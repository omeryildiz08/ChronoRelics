# eMERGEncy - Yazilimci Juri Sunum Notlari

Hazirlayan: Omer Yildiz  
Rol: Game Developer  
Proje: eMERGEncy  
Sunum hedefi: Juriye projedeki yazilim katkilarimi, oyun mimarisini, mekaniklerin teknik karsiligini ve demo sirasinda gosterebilecegim akis noktalarini net anlatmak.

## 1. Kisa Tanitim

eMERGEncy, zaman yolculugu temasina sahip izometrik bir merge oyunudur. Oyuncu farkli caglara ait objeleri grid uzerinde birlestirerek daha ust seviye objelere ulasir, level hedefini tamamlar ve odulleri ana usse tasir.

Ben projede oyun mekaniklerinin kodlanmasindan ve sistemlerin birbirine baglanmasindan sorumluyum. Ana odagim grid tabanli merge sistemi, level akis sistemi, save/load, market, kaynak sistemleri, facility power ve anomaly mekaniklerinin calisir hale getirilmesi oldu.

## 2. Yazilim Tarafinda Yaptigim Ana Isler

### Grid ve Merge Sistemi

Oyunun temel mekanigi grid uzerinde calisiyor. Her tile bir `GridTileView` ile temsil ediliyor ve runtime tarafinda `GridManager` icindeki iki boyutlu grid datasina kaydediliyor.

Merge sistemi su sekilde calisiyor:

- Oyuncu objeyi surukleyip bir tile'a birakir.
- Hedef tile gecersizse, kilitliyse veya merge icin uygun degilse obje eski yerine doner.
- Hedefte ayni item varsa sistem 4 yonlu komsulukla bagli item grubunu arar.
- Ayni itemdan 3 veya daha fazla bagli obje varsa merge gerceklesir.
- Merge sonucu, itemin `NextLevelItem` datasindaki prefabdan yeni obje uretilir.
- Merge tamamlandiginda diger sistemlere event gonderilir.

Bu yapida merge sistemi sadece item saymakla kalmiyor; level hedefi, quest ilerlemesi ve anomaly tetiklenmesi gibi sistemlere de event sagliyor.

### ScriptableObject Tabanli Item Mimarisi

Item verilerini koddan ayirmak icin `MergeableItemData` ScriptableObject yapisini kullandim.

Her item datasinda:

- Benzersiz `ItemID`
- Oyunda gorunen `ItemName`
- Item seviyesi
- Prefab referansi
- Bir sonraki merge seviyesi
- Satis fiyati
- Market alis fiyati
- Facility Power degeri

bulunuyor.

Bu sayede Game Designer veya ekipteki baska biri kod yazmadan yeni merge zincirleri kurabiliyor. Ornegin bir itemin bir sonraki seviyesini degistirmek icin sadece Inspector uzerinden `NextLevelItem` alanini degistirmek yeterli.

### Level Akisi

Level sistemi `LevelManager` uzerinden calisiyor.

Bir level sahnesinde:

- `TargetItem` level hedefini belirliyor.
- Oyuncu bu itemi merge sonucu uretirse level tamamlanmis sayiliyor.
- `RewardItem` oyuncunun inventorysine ekleniyor.
- `ChronoChargeReward` varsa oyuncuya ekstra kaynak veriliyor.
- Level complete paneli aciliyor.
- Oyuncu devam edince base sahnesine donuyor.

Bu akista level completion direkt merge eventine bagli. Yani sistem surekli sahneyi taramak yerine merge sonucu uretilen itemi dinliyor. Bu daha temiz ve performansli bir yapi sagliyor.

### Base Akisi ve Odul Yerlesimi

BaseScene, oyuncunun level odullerini topladigi ve itemlarini sakladigi ana alan olarak calisiyor.

`BaseManager` base sahnesi acildiginda:

- Grid kayitlarinin tamamlanmasini bekliyor.
- Save dosyasindan base durumunu yukluyor.
- Inventoryde bekleyen level odullerini ilk bos ve kilitsiz tile'a yerlestiriyor.
- Yerlestirme sonrasi save aliyor.

Bu sayede level sonunda kazanilan odul, base sahnesine donuldugunde otomatik olarak oyunun fiziksel alanina ekleniyor.

### Save / Load Sistemi

Save sistemi `SaveManager` ve `GameSaveData` uzerinden calisiyor.

Kaydedilen veriler:

- Base uzerindeki objelerin item ID'leri ve grid pozisyonlari
- Kilitli tile listesi
- Baslangicta kilitli olup oyuncu tarafindan acilmis tile listesi
- Inventoryde bekleyen oduller
- Time Credit
- Chrono Charge

Save dosyasi JSON olarak `Application.persistentDataPath` altina yaziliyor.

Burada onemli nokta su: Level sahnesindeyken base state ezilmiyor. Level icinde sadece inventory ve ekonomi verileri guncelleniyor. BaseScene'de ise tam base durumu kaydediliyor. Bu ayrim, leveldan donerken base yerlesiminin bozulmamasini sagliyor.

### Kapali Grid Sistemi

Kapali grid mekanigi GDD'deki onemli engel mekaniklerinden biri. Bunu tile seviyesinde `isLocked` state'i ile kurdum.

Kilitli tile davranislari:

- Oyuncu kilitli tile'a obje birakamaz.
- Kilitli tile uzerindeki obje suruklenemez.
- Ancak kilitli tile uzerindeki obje merge grubuna dahil olursa tile acilir.
- Kilit acildiginda tile materyali normal hale doner.

Bu sistem hem level puzzle tasariminda hem de base expansion mekaniginde kullaniliyor.

### Facility Power Sistemi

Facility Power, base gelisimi icin kullandigimiz ilerleme kaynagi.

Sistem su sekilde calisiyor:

- Base uzerindeki her item kendi `FacilityPoint` degerini verir.
- `FacilityPowerManager` base gridindeki itemlari tarayarak toplam FP'yi hesaplar.
- Acilmis expansion tile'lari kullanilmis FP olarak dusulur.
- Oyuncu yeterli kullanilabilir FP'ye sahipse kilitli base tile'a tiklayarak acabilir.

Burada GDD'deki "belirli FP esiginde base genisler" fikrini daha kontrollu bir sisteme cevirdim. Oyuncu tek tek tile acabiliyor. Bu hem tasarimciya daha detayli kontrol veriyor hem de oyuncuya hangi bolgeyi once acacagini secme hissi veriyor.

### Time Credit ve Market Sistemi

Market sistemi base icinde Time Credit ekonomisiyle calisiyor.

Oyuncu:

- Satilabilir objeleri sell zone'a surukleyerek Time Credit kazanabiliyor.
- Market panelinden `BuyPrice` degeri olan itemlari satin alabiliyor.
- Satin alinan item ilk bos uygun base tile'ina spawn oluyor.

Satilamaz durumlar:

- Satis fiyati 0 olan item
- Kilitli tile uzerindeki item
- Inactive anomaly item
- Item datasi eksik obje

Bu sistem, oyuncuya hem ekonomi hem de soft-lock cozumu sagliyor. Base doldugunda veya istenmeyen itemlar biriktiginde oyuncu satis yaparak alan acabiliyor.

### Chrono Charge Sistemi

Chrono Charge, level girislerinde harcanan kaynak olarak calisiyor.

`LevelSelector` uzerinden:

- Her level icin farkli Chrono Charge maliyeti verilebiliyor.
- Varsayilan level maliyeti belirlenebiliyor.
- Oyuncunun yeterli kaynagi yoksa level acilmiyor.
- Level tamamlaninca `ChronoChargeReward` ile kaynak geri kazanilabiliyor.

Bu sistem level secimini ekonomiye baglamak ve oyuncunun ilerleme ritmini kontrol etmek icin kuruldu.

### Anomaly Sistemi

Anomaly sistemini iki tipe ayirdim.

#### Type 1 - Random Rift

Her merge sonrasi belirli bir yuzdeyle rastgele bir acik tile kilitlenebiliyor.

Type 1:

- Merge sonrasi tetiklenir.
- `anomalyChance` degeriyle ihtimal kontrolu yapar.
- Uygun acik tile'lardan birini secer.
- Tile'i kilitler.
- Rift efektlerini spawn eder.
- Tile acilinca efektleri temizler.

Bu mekanik levela dinamik risk ekliyor. Oyuncunun planini bozabiliyor ama tamamen rastgele oldugu icin dar levellarda dikkatli kullanilmasi gerekiyor.

#### Type 2 - Orb Rift

Type 2 daha kontrollu anomaly sistemi. Tasarimci level sahnesinde belirli tile'lari anomaly orb olarak isaretliyor.

Orb su anlama geliyor:

- Leveldaki anomaly kaynagi.
- Merge sonrasi belirli ihtimalle aktiflesir.
- Kendi cevresindeki 3x3 alana etki eder.
- Etkileri agirlik sistemiyle secilir.

Type 2 etkileri:

- Acik tile kilitleme
- Kilitli tile icindeki objeyi baska bos tile'a tasima
- Baska caga ait pasif item spawn etme
- Orb'un baska bir tile'a isinlanmasi

Burada agirlik sistemi kullandim. Yani tasarimci "bu orb daha cok kilitlesin" veya "daha cok teleport etsin" gibi ayarlari Inspector'dan yapabiliyor.

### Quest Sistemi

Quest sistemi level icindeki yan hedefleri gostermek icin kuruldu.

Mevcut calisan kisim:

- `ProduceItem` tipi questler var.
- Oyuncu belirlenen itemi urettikce progress artiyor.
- Gerekli sayiya ulasinca UI'da tamamlandi olarak isaretleniyor.

Bu sistem su an level bitirme kosuluna bagli degil. Daha cok yan hedef / yonlendirme sistemi gibi calisiyor.

## 3. Genel Mimari

Projede sistemleri mumkun oldugunca birbirinden ayirarak kurdum.

Ana yapi:

- `GridManager`: grid, merge, tile state ve base state eventleri.
- `MergeableObject`: oyuncu inputu, drag/drop ve item objesi davranisi.
- `MergeableItemData`: item verisi.
- `LevelManager`: level hedefi ve odul akisi.
- `SaveManager`: kalici veri ve item database lookup.
- `BaseManager`: base sahnesi yukleme ve inventory odulu yerlestirme.
- `MarketManager`: satin alma ve satis.
- `FacilityPowerManager`: base genisletme ve FP hesabi.
- `AnomalyManager`: Type 1 ve Type 2 anomaly etkileri.
- `LevelQuestManager`: quest progress takibi.

Sistemler arasi baglanti genellikle eventler ve singleton managerlar uzerinden ilerliyor.

Ornek event akisi:

1. Oyuncu merge yapar.
2. `GridManager` merge'i tamamlar.
3. `OnMergeCompleted` event'i yayinlanir.
4. `LevelManager` hedef item kontrolu yapar.
5. `LevelQuestManager` quest progress kontrolu yapar.
6. `AnomalyManager` anomaly tetikleme kontrolu yapar.
7. BaseScene'deyse save ve FP hesaplari guncellenir.

Bu event tabanli akis, sistemlerin birbirini surekli sorgulamasini azaltir.

## 4. Demo Sirasinda Gosterilebilecek Akis

### 1. Merge Mekanigi

Gosterilecekler:

- Ayni itemdan 3 tanesini yan yana getir.
- Merge sonucu bir ust seviye itemin olustugunu goster.
- Merge animasyonu ve ses/efekt varsa bunlari vurgula.

Anlatim:

"Burada grid uzerindeki objeler kendi ScriptableObject verileriyle calisiyor. Ayni itemlardan 3 veya daha fazlasi 4 yonlu komsulukla bagliysa merge gerceklesiyor ve itemin NextLevelItem referansindan yeni obje uretiliyor."

### 2. Kilitli Tile

Gosterilecekler:

- Kilitli tile uzerindeki objenin suruklenemedigini goster.
- Kilitli tile'daki objeyi merge grubuna dahil ederek tile'in acildigini goster.

Anlatim:

"Kapali gridler oyuncunun dogrudan hareketini engelliyor ama merge planlamasiyla acilabiliyor. Bu sistemi hem level engeli hem de base expansion icin ortak kullaniyorum."

### 3. Level Completion

Gosterilecekler:

- Target item uret.
- Level complete UI gelsin.
- Reward ve Chrono Charge odulunu anlat.
- Base'e donuste reward itemin grid'e yerlestigini goster.

Anlatim:

"Level hedefi direkt merge eventinden kontrol ediliyor. TargetItem uretildiginde level tamamlanmis sayiliyor, reward inventoryye ekleniyor ve base sahnesine donunce otomatik olarak ilk uygun tile'a yerlestiriliyor."

### 4. Market ve Time Credit

Gosterilecekler:

- Bir objeyi sell zone'a surukleyip sat.
- Time Credit artisini goster.
- Marketten item satin al.

Anlatim:

"Market sistemi Time Credit ekonomisiyle calisiyor. Itemlarin satis ve alis fiyatlari ScriptableObject uzerinden ayarlaniyor. Bu sayede ekonomi dengesi kod yazmadan degistirilebiliyor."

### 5. Facility Power

Gosterilecekler:

- FP UI degerini goster.
- FP veren objeyi base'e koyunca degerin arttigini anlat.
- Kilitli expansion tile'a tiklayip ac.

Anlatim:

"Base'deki objeler Facility Power uretiyor. Oyuncu yeterli kullanilabilir FP'ye sahipse kilitli base tile'larini acabiliyor. Acilan tile'lar kullanilmis FP olarak dusuluyor."

### 6. Anomaly

Gosterilecekler:

- Type 1 icin merge sonrasi tile kilitlenmesini goster.
- Type 2 icin orb etrafinda 3x3 etki alanini anlat.
- Inspector'da chance ve weight alanlarini goster.

Anlatim:

"Anomaly sistemini iki tipe ayirdim. Type 1 daha rastgele, merge sonrasi tile kilitleyen sistem. Type 2 ise level designer tarafindan yerlestirilen orb'lar uzerinden calisiyor ve 3x3 alanda farkli etkiler uretebiliyor."

## 5. Teknik Olarak Vurgulanabilecek Noktalar

- Veriyi koddan ayirmak icin ScriptableObject kullandim.
- Merge sonrasi sistemleri event tabanli bagladim.
- Save/load sistemini base ve level sahneleri icin farkli davranacak sekilde ayirdim.
- Market, Facility Power, Chrono Charge gibi ekonomileri merkezi `SaveManager` ile kalici hale getirdim.
- Kilitli tile sistemini hem level puzzle hem base expansion icin tekrar kullanilabilir tasarladim.
- Anomaly Type 2'de agirlikli rastgele secim ve 3x3 lokal etki alani kullandim.
- Designer'in Inspector'dan ayarlayabilecegi alanlari mumkun oldugunca acik tuttum.

## 6. Sunumda Kullanilabilecek Kisa Konusma Metni

"Ben projede game developer olarak mekaniklerin kodlanmasindan ve sistemlerin birbirine baglanmasindan sorumluyum. Oyunun temeli grid tabanli merge sistemi. Her item bir ScriptableObject verisine sahip ve bu veri uzerinden prefab, merge sonrasi olusacak item, satis fiyati, alis fiyati ve Facility Power gibi degerler ayarlanabiliyor.

Merge sistemi 4 yonlu komsulukla en az 3 ayni itemi buluyor. Merge tamamlandiginda bir event yayinliyorum. Bu event level hedefi, quest ilerlemesi ve anomaly mekanigi tarafindan dinleniyor. Boylece sistemler birbirine direkt bagimli olmadan ayni merge sonucuna tepki verebiliyor.

Level tarafinda hedef item uretildiginde level tamamlanmis sayiliyor. Oyuncuya reward item ve varsa Chrono Charge veriliyor. Reward inventoryye kaydediliyor ve base sahnesine donunce otomatik olarak griddeki ilk uygun alana yerlestiriliyor.

Base tarafinda save/load, market, Time Credit, Chrono Charge ve Facility Power sistemleri var. Oyuncu item satip Time Credit kazanabiliyor, marketten item alabiliyor ve base'deki itemlarin verdigi Facility Power ile yeni tile'lar acabiliyor.

Anomaly sisteminde iki farkli tip kurdum. Type 1 merge sonrasi rastgele tile kilitleyen daha kaotik bir sistem. Type 2 ise level designer'in sahneye koydugu orb'lar uzerinden calisiyor. Bu orb'lar merge sonrasi belirli ihtimalle aktiflesip 3x3 alanlarinda tile kilitleme, obje tasima, foreign item spawn etme veya teleport gibi etkiler uretebiliyor.

Genel olarak amacim, mekanikleri sadece calisir hale getirmek degil, ayni zamanda tasarimci arkadasimin Inspector uzerinden level ve ekonomi dengesi kurabilecegi esnek bir mimari olusturmakti."

## 7. Juri Sorularina Hazir Cevaplar

### "Neden ScriptableObject kullandiniz?"

Item verilerini koddan ayirmak icin. Boylece yeni item eklemek, merge zinciri kurmak, fiyat veya FP degeri degistirmek icin kod degistirmeye gerek kalmiyor. Bu da tasarimci icin daha hizli iterasyon sagliyor.

### "Merge sistemi nasil karar veriyor?"

Oyuncunun itemi biraktigi pozisyondan baslayarak ayni `ItemData` referansina sahip objeleri 4 yonlu komsulukla ariyor. Grup 3 veya daha fazlaysa merge yapiliyor.

### "Save sistemi hangi problemi cozuyor?"

Base sahnesindeki item yerlesimini, kilitli/acilmis tile durumlarini, inventoryyi ve ekonomiyi kalici hale getiriyor. Ayrica level sahnesinde base state'in yanlislikla ezilmesini engellemek icin level ve base save davranislarini ayirdim.

### "Facility Power nasil calisiyor?"

Base'deki itemlarin `FacilityPoint` degerleri toplanir. Acilmis expansion tile'lari kullanilmis FP olarak dusulur. Kalan kullanilabilir FP yeterliyse oyuncu kilitli base tile'ini acabilir.

### "Anomaly orb nedir?"

Orb, Type 2 anomaly mekaniginin level icindeki kaynagidir. Designer belirli tile'lari orb olarak ayarlar. Her merge sonrasi bu orb belirli ihtimalle aktiflesir ve cevresindeki 3x3 alanda anomaly etkisi uretir.

### "Sistemde eksik veya gelistirilebilir noktalar neler?"

Completed level progression, questlerin level completion'a zorunlu olarak baglanmasi, anomaly state'in save edilmesi ve market icon destegi gelistirilebilir. Temel mekanikler calisiyor, ancak bu alanlar urunlesme icin sonraki adimlar.

## 8. Sunum Icin Onemli Dosyalar

- `Assets/Scripts/Grid/GridManager.cs`: grid ve merge omurgasi.
- `Assets/Scripts/MergeableObject.cs`: item surukleme ve birakma davranisi.
- `Assets/Scripts/SO/MergeableItemData.cs`: item data modeli.
- `Assets/Scripts/LevelManager/LevelManager.cs`: level hedef ve odul akisi.
- `Assets/Scripts/SaveSys/SaveManager.cs`: save/load ve ekonomi state'i.
- `Assets/Scripts/BaseManager.cs`: base yukleme ve inventory odul spawn.
- `Assets/Scripts/Market/MarketManager.cs`: market ve satis.
- `Assets/Scripts/FacilityPower/FacilityPowerManager.cs`: FP ve base expansion.
- `Assets/Scripts/AnomalyManager.cs`: Type 1 ve Type 2 anomaly sistemi.
- `Assets/Scripts/Quest_Scripts/LevelQuestManager.cs`: quest progress takibi.

## 9. Kapanis Cumlesi

"Bu projede yazilim tarafinda odagim, merge oyununun temel mekaniklerini calisir hale getirirken tasarimcinin de Unity Inspector uzerinden level, ekonomi ve anomaly ayarlarini rahatca yapabilecegi bir yapi kurmakti. Grid, item data, level akisi, save/load, market, Facility Power ve anomaly sistemleri bu amacla birbirine baglanmis durumda."
