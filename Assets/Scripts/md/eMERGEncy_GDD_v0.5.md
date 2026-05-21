# eMERGEncy — Oyun Tasarım Dokümanı (v0.5)

## Ekip
- Zehra Yağmur Taşkesen – 3D Artist
- Ayşenur Yerinde – 2D Artist
- Ömer Yıldız – Game Developer
- Bahadır Turgun – Game Designer

## Revizyon Geçmişi
| Değişikliği Yapan | Yapılan değişiklik | Revizyon Numarası | Değişiklik Tarihi |
|---|---|---|---|
| Bahadır Turgun | İlk Versiyon | v0.1 | 12/11/2025 |
| Bahadır Turgun | Hikaye eklendi, farklı başlıklar düzenlendi. | v0.2 | 13/11/2025 |
| Bahadır Turgun | Target Audience Analysis, Game Motivation Model, Market Research, Game Loop Diagrams, UI Flow Diagrams, Risk Analysis and Alternative Plan başlıkları ve yeni referans görseller eklendi. | v0.3 | 22/11/2025 |
| Bahadır Turgun | Core Gameplay Mechanics ve konsept mekanikler revize edildi.<br>Gameplay başlığı altında Chrono Charge, Anomaly Market, Facility Power, Kapalı Grid, Anomali Zaman Yarığı ve Quest sistemi kısımları eklendi.<br>Yeni referans görseller eklendi. | v0.4 | 8/12/2025 |
| Bahadır Turgun | Genel Değişiklikler yapıldı. Time Credit eklendi. Assets kısmı tamamıyla güncellendi. Destekleyici görseller eklendi | v0.5 | 16/01/2026 |

## Overview
### Theme / Setting / Genre
- Zaman yolculuğu
- Gelecekte bilim insanlarının geliştirdiği bir saat ile gelecekte başlayıp, çeşitli zaman ve mekanlarda geçen bir evren
- Merge 3
### Core Gameplay Mechanics Brief
- Merge 3 Mekaniği
### Targeted Platforms
- PC/Mobile
### Publishing Platforms
- Steam
- Google Play Store
### Art Style
- Low-Poly, Hand painted Textures
### Perspective
- İzometrik Bakış Açısı
### Project Scope
- Game Time Scale
- Time Scale / 7 Months
- Team Size
- Zehra Yağmur Taşkesen
- 3D Artist

Ayşenur Yerinde
2D Artist

Ömer Yıldız
Game Developer

Bahadır Turgun
Game Designer

### Market Research
- Merge Dragons! --- Gram Games (2017)
- Ana Oyun Mekanikleri tasarlanırken örnek alınan oyun.

EverMerge --- Big Fish Games (2020)
Temel merge mekanikleri için referans alınan oyunlardan biri

### Target Audience Analysis
- eMERGEncy, bulmaca çözmekten keyif alan, kısa oyun oturumlarını tercih eden ve bir hikâye deneyimlemekten hoşlanan Casual ve Mid-Core oyuncu kitlesini hedef alan bir oyundur. Oyunun hem PC hem de mobil platformlarda sunulması, erişilebilirliği en üst düzeyde tutarak oyuncu grupları arasında yayılmayı amaçlar.

Demografik Kitle: Oyunun Merge mekaniği genellikle öğrenmesi kolay fakat ustalaşması zor bir mekanik olduğundan dolayı geniş bir yaş aralığına hitap eder. Bu mekanik çevresinde daha çok 25-45 yaş aralığı hedef alır. Bu yaş aralığı özellikle mobil platformlarda bu türe en çok zaman ayıran kitledir.
Bunun yanı sıra oyunun bilim kurgu/zaman yolculuğu tarafı ise daha çok 18-25 yaş aralığına hitap eder. Bu yaş aralığı ise daha çok PC platformu üzerinden oyunlara zaman ayırır.

Oyuncu Tipi: eMERGEncy, oyuncu tiplemeleri arasından en çok başaran (Achiever) ve Düzenleyici (Organizer) oyuncu tiplemelerine hitap eder. Grid üzerindeki dağınıklıkları birleştirerek ortalığı toplamak ve her şeyi yerli yerince düzenlemek, organizer oyuncu tiplemesini oyunun ana oyuncu tipi haline getirir. Oyuncunun karşılaştığı bölümleri tamamlayarak bir sonrakine geçmesi ve bu döngüde oyunu sürdürmesi ise başaran oyuncu tiplemesinin oyunun ikincil oyuncu tipi olmasına sebep olur.

Platform Davranışları
Mobil: oyunun mobil platformlarda bulunması ve kısa oyun döngülerinden oluşması; toplu taşımada, molalarda ve kısa bekleme süresi gerektiren her anda oyunu oynayan kişi için idealdir.

PC: PC platformunda bu oyun türü için yeterli oyun bulunmamaktadır. Talep ne kadar mobil platformlara göre küçük olsa da PC platformu için yeterli arz piyasada yer almamaktadır. eMERGEncy ise bu boşluğu kapatmak için ve özellikle 18-25 yaş aralığını hedef kitlesinde tutmak amacıyla PC platformunda yer alır.

### The Elevator Pitch
- Farklı çağlarda zaman taşlarını topla, yeni çağların kapılarını aç ve hikâyeye tanıklık et!

### Project Description (Detailed)
- Project eMERGEncy, oyuncunun Farklı çağlara ait objeleri birleştirerek yeni nesneler elde ettiği ve bu nesneler ile tekrar birleştirme yaparak bölümü tamamlamaya çalıştığı bir “Merge 3” oyunudur.
- Oyuncu, her bölümde farklı bir çağda bulunur. O döneme ait çeşitli objeler ile birleştirme yaparak farklı objeler elde eder. Bölüm sonlanana kadar bu döngü devam eder. Döngünün sonunda ise hikâyeyi devam ettirmek için gerekli olan objeyi elde eder. Döngü sonunda elde edilen bu objeler “Main Base” üzerinde farklı birleştirmeler yapmak ve hikâye için gerekli olan nesneleri temin etmek için kullanılır.
- Her bölüm içerisinde birbirinden farklı zaman ve mekanlara ziyaret eden oyuncu, yeni bölgeleri keşfetmek ve hikâyeyi devam ettirmek için oyuna devam eder.

### Core Gameplay Mechanics (Detailed)
- “Merge 3” Mekaniği
- Oyuncu “grid” üzerinde bulunan aynı 3 objeyi bir araya getirerek yeni bir obje elde eder. Yeni elde edilen bu objeden toplam en az 3 tane elde eden oyuncu yeni bir birleştirme yapma şansı yakalar. Zaman ve mekâna göre değişen bu objeler belirli bir birleştirme ağacını takip eder. Oyuncunun hedefi, bölüm içerisinde istenilen birleştirme basamağına ulaşmaktır.

Oyuncuya bölüm sonunda verilen obje, “Main Base” içerisine yerleştirilir. Yerleştirilen bu objeler yeni zaman taşına ulaşmak için veya hikâye parçacıkların ortaya çıkarmak için kullanır.

Oyun base içerisinde çeşitli birleştirmeler yaparak merge ağacının sonuna ulaşmaya çalışır. Sınırlı grid içerisinde zamanla üssünü geliştirerek daha geniş alana sahip olan oyuncu, dilediği gibi birleştirme yaptığı objeleri Zaman kredisi karşılığında satabilir.

Oyuncu, ilerlemek istediği zaman dilimi içerisinde bölüme giriş yapmalıdır. Zaman değiştirmek için kaynak harcaması gereken oyuncu, farklı ödül çeşitleri sayesinde bu kaynağı elde edebilir.

Sol tık ile objeler tutulup bırakılabilir ve diğer gridlere yerleştirilebilir. Kapalı gridlere yerleştirme yapılamaz. Kapalı grid açılır ise yerleştirme yapılabilir.

Bu döngü içerisinde oyun, hikâye bitene kadar devamlılığını sürdürür.

### Game Loop Diagrams

### Game Motivation Model
- eMERGEncy, oyuncuların temel olarak başarma/tamamlama (Sense of Completion) hissi ve keşfetme arzusu (Curiosity) ile oyuncuyu oyunda tutmayı amaçlar. Hedef kitledeki oyuncular, gridler üzerinden dağınık olan bölümleri birleştirme yardımı ile düzenleyerek bilişsel rahatlamayı yaşarken bir yandan da bir hikâyeye tanık olmaktadır.
- Kaostan düzen yaratmak bu oyun türünün psikolojik temelidir. Diğer oyunların dışında, dekorasyon yerine hikâye odağı eMERGEncy’yi diğer oyunlardan ayırır.
- Bu maddeler çerçevesinde oyunculara “ben bu oyunu oynamak istiyorum” düşüncesi yerleştirilmesi amaçlanmaktadır.
## Story and Gameplay
### Story (Detailed)
- Gelecekte bilim insanları, zamanda seyahat edebilmek bir saat geliştirirler. 6 farklı zaman taşını kullanarak çalışan bu saat, kişiyi seçtiği zaman ve mekâna gönderen bir mekanizmadır. Karakterimiz bu saati denemek için gönüllü olan deneklerden biridir.  Deney zamanı geldiğinde karakterimiz saati takar ve deney başlar. Taşların sırasının yanlış dizilmesi dolayısıyla karakterimiz bilmediği bir zamana ışınlanır. Elindeki saatte sadece bir zaman taşı olduğunu fark eder karakterimiz geri dönmenin yollarını arar.

Geri kalan zaman taşları 5 farklı zamana dağılmıştır. Oyuncu, kendi zamanında dönebilmek için geri kalan 5 taşı da toplamak zorundadır. Bu sayede saat, tam fonksiyon çalışan haline geri getirebilir. Oyuncu bu bilgileri oyunda ilerledikçe elde eder. Bölüm sonları veya çeşitli noktalarda zaman taşı aracılığıyla görüler görerek taşlar hakkında bilgi toplar.

Zaman taşları birbirleri ile uyum içinde çalışır. Karakterimizde kalan zaman taşı, kendini tamamlayabilmek için diğer taşların olduğu zaman dilimlerine oyuncuyu yönlendirir. Fakat taşlar bir arada olmadığından zaman ve mekân seçimi oyuncuya ait değildir.

### Gameplay (Detailed)
- Oyuncu oyuna bir bölüm içerisinde başlar. Mouse sol tık veya dokunmatik ekranı kullanarak aynı objeleri birbirleri üzerine sürükleyip bırakarak merge mekaniğini devreye sokar. Bunun sonucunda ortadaki objenin konumunda yeni bir obje meydana gelir. Oyuncu Aynı şekilde tıklayıp bırakarak objelerin yerlerini diğer gridlerin üzeri olacak şekilde değiştirebilir. Oyuncu bu döngüde istenilen aşamaya kadar merge mekaniğini kullanarak ilerler. İstenilen obje elde edildiğinde bölüm sona erer ve “Bölüm Sonu Ekranı” belirir. Oyuncu “devam” butonuna tıklayarak bölümü terk eder.

Bölüm bitişinin ardın “Main Base” sahnesi açılır. Ardından kazanılan objelerin gözüktüğü bir sekme açılır. Oyuncu oradaki objeleri gridlere sürükleyip bırakarak yerleştirir. Bu objeler bir birleştirmede kullanılana kadar sahnede kalmaya devam eder.

Oyuncunun amacı, bölümleri tamamlayarak gerekli olan tüm birleştirme objelerini toplamak ve hikâyeyi deneyimlemektir.

### Chorono Charge
- Oyuncunun zaman değiştirmek için kullanacağı kaynak türüdür. Zaman değişimi, bölüme başlamadan önce ana üs kısmından değiştirilir. Bu kaynak çeşitli bölüm sonları, Anomaly Market ve Charge Generator binasından elde edilebilir.

Örn: Zaman değiştirmek 1 adet CC Kullanır. 1,4,9,12 ve 15. seviyeleri tamamlamak, oyuncuya 1 tane zaman CC kazandırır

### Time Credit
- Oyuncu, tamamladığı seviyeler neticesinde kazandığı ve base içerisinde birleştirmede kullandığı objeleri “Time Credit” karşılığında satabilir. Her objenin kendi karşılığı vardır. Bu kaynak daha sonra “Anomaly Market” içerisinde kullanılabilir.

### Facility Power
- Oyuncunun üssüne inşa ettiği binalar ve yerleştirdiği objeler, üssün Facility power’ını yükseltir. Bu kaynağın belirli bir seviyeyi geçmesi haritanın genişlemesine ve daha fazla birleştirme alanı açılmasına sebep olur.

Örn: A objesin 3, b objesin 7 ve c objesinin 15 FP verdiği bir senaryoda; 2 adet a, 1 adet b ve 2 adet c objesine sahip olalım. Bu senaryoda toplam 43 FP’ye sahip oluruz. 45 FP’yi barem olarak alalım. Oyuncunun bir adet a daha yerleştirmesi halinde üs, 5x5 genişliğinden 6x6 genişliğine yükselir.

Sırasıyla: Chrono Charge – Time Credit – Facility Power

### Anomaly Market
- Oyuncu üs içerisinden istemediği çeşitli objeleri “Zaman Kredisi” karşılığında satabilir. Bu kredilerle birlikte düşük seviye olmak şartı ile çeşitli zamanlardan farklı objeler satın alabilir.

Örn: 3 adet “c” objesi ile oluşturulmuş bir “d” objesi, 5 zaman kredisi karşılığında satılır. Oyuncu marketten 17 kredi karşılığında bir “e” objesi satın alabilir.

### Kapali Grid
- Oyuncunun renk farklılığı ile ayırt edebileceği bu grid türü, içerisindeki objeyi hareket ettirilemez hale getirir. Sadece içerisindeki obje ile bir birleştirme yapıldığında obje kullanılabilir. Birleştirme sonucunda ise grid açık hale gelir.

### Anomali̇ Zaman Yariği
- 2 tip anomali tipi vardır.
- İlk anomali çeşidi. Merge objelerinin birleşmesinin ardından şansa dayalı olarak herhangi bir grid içerisinde var olabilir. Anomali etkisine giren grid üzerinden çeşitli efektler belirir.

İkinci tip ise anomali toplarıdır. Bu anomali yarıkları bölümler içerisinde önceden ayarlanmış gridlerde bulunur. Bu yarıklar etrafındaki 3x3 alan içerisine etki eder. Her bir birleştirmenin ardından belirli bir yüzdelik ile yarık aktifleşir.

Anomali yarıkla Özellikleri:
Açık bir gridi kapalı hale getirebilir.
Kapalı grid içerisindeki bir objenin yerini değiştirebilir. (Tip 2)
Farklı zamana ait bir objeyi bölüme yerleştirir (nesne işlevsiz)
Kendini başka bir gride ışınlayabilir. (Tip 2)

### Quest Si̇stemi̇
- Oyuncu, bölüm içerisinde farklı yan objektiflere sahiptir. Bu objektifleri tamamlaması halinde ek ödüller kazanabilir. Bir ana objektifin haricinde bir veya birden fazla yan objektif bulunabilir.
- Ana üs içerisinde ise görevler oyuncuyu yönlendirmek amacıyla vardır. Oyuncunun ne yönde ilerlemesi gerektiği çeşitli görevler aracılığı ile aktarılır.

Level içerisinde ise questlerin işleyişi farklıdır. Bölümü tamamlamak ve ödülü kazanabilmek için questleri tamamlamak gerekmektedir.

### Konsept Mekani̇kler

Zaman kırılması: Oyuncu, yaptığı merge sayısında belirtilen limiti aşarsa haritanın belirli bölümleri erişime kapanır. Oyuncu, birleştirme yaparken olabildiğince toplu sayıda objeyi tek seferde birleştirmeyi amaçlamalıdır.

Research binası: Oyuncu çeşitli bölümlerden elde ettiği runic veya benzeri bir objeyi bu binada araştırır. Araştırma sonucunda yeni hikâye parçası veya ipucu elde eder.

### UI Flow Diagrams

### Risk Analysis and Alternative Plans
- Risk 1: Kapsam Genişlemesi (Scope Creep) – Risk Düzeyi yüksek – Etki Kritik
- Oyun içerisinde 5 farklı zaman dilimi olduğundan dolayı bu zaman dilimlerinin her biri için farklı modeller, sesler ve efektler üretmekte sorunlar oluşabilir. Her bir zamanın birbirinden farklı olması gerekmekte.
- Böyle bir sorun oluştuğunda ise yapılabilecek 2 farklı seçenek vardır.
- Hazır assetler aracılığı ile eksik kalan kısımları kapatmak
- Zaman dilimi sayısını 3’e veya 4’e düşürmek.

Risk 2: Sanat Dili Uyuşmazlığı – Risk Düzeyi Orta – Etki Yüksek
Geliştirme ekibinden 3b ve 2b assetler farklı kişilerce oluşturulacağından, çeşitli uyumsuzluklar ortaya çıkabilir.
Bu sorunun oluşmaması için yapılabilecek şeyler
Proje başlangıcında katı bir Renk paleti ve Stil Rehberi oluşturulabilir.
Proje süresince iki tarafında birlerinden maksimum şekilde haberdar olması sağlanabilir.

Risk 3: Merge (Birleştirme) Algoritması Hataları – Risk Düzeyi düşük – Etki Kritik
Oyun geliştirilirken grid sisteminin hatalar barındırabilme ihtimali mevcuttur. Birleştirme ve grid sistemi üzerinde dikkatli olunmalıdır.
Bu sorunun oluşmaması için yapılabilecek şeyler
“Polish” için gereken sürenin sağlanması
Geliştirme süresince sürekli Play-Test yapılmalı ve hatalar olabildiğince erken saptanmalıdır.
Risk 4: Takım İçi İletişim Kopukluğu ve motivasyon – Risk Düzeyi Orta – Etki yüksek
Geliştirme süresi 7 ay gibi ciddi bir süre olduğunda dolayı zamanla motivasyon kaybı veya ekip içi iletişim kopuklukları yaşanabilir.
Bu sorunun oluşmaması için yapılabilecek şeyler
Haftalık düzenli toplantılar yaparak iletişimde kalmak
Gerekli yerlerde projeye ara verilebilir
Takım içi haberleşmeyi üstlenecek birini seçmek (örn: Game Designer)

Risk 5: Grid Kilitlenmesi (Deadlock / Soft-lock) – Risk Düzeyi Düşük – Etki Kritik
Eğer oynanış planlanması düzgün yapılması grid veya obje sayısıyla ilişkili olarak oyuncu “Soft-Lock” içerisinde kendini bulabilir. Birleştirme yapacak yerin kalmaması gibi çeşitli durumlar baş gösterir.
Bu sorunun oluşmaması için yapılabilecek şeyler
Düzenli Play-Test yapmak
Oyuncunun gridleri boşaltabilmesi için objeleri satma mekaniği
Soft-Lock durumuna yakın bir noktaya gelindiğinde oyuncuya gerekli bir uyarı yapmak.
## Assets Needed
### 2D
- Textures
- Grid

İcons
Chrono Charge
Facility Power
Time Credit
Anomaly Market
Level İcon

Images
Background image (cloudy sky)
Button
### 3D
- Merge Block List
- Hançer
- Kılıç
- Mızrak
- Balta
- Kalkan
- Gürç
- Çekiç
- Simya otu
- Dikenli Simya Otu
- Mantar
- Tarif Kitabı
- Boş İksir Şişesi
- Dolu İksir Şişesi
- Büyülü İksir Şişesi

### Sound
- Merge (3)
- Move error (2)
- Level complete (1)
- Background Music (1)
- Paper/Sheet/Parchment (3)
- Button (2)

### Code
- Grid sistemi
- Merge
- Save/Load
### Sfx/Vfx
- Quest
### So
### Animation
- Merge
- Ara sahneler

### Vfx
- Merge
- Button
- Anomaly Explosion
- Wind Lines
- Firefly
- Flying Stars
- Cloud
- Level Complete (confetti)
- Quest Complete (Star popping)

### References and Artworks

Anomali topu

Görsel tarz referansları

### Schedule
- https://miro.com/app/board/uXjVGe6CpMg=/

## Mevcut Implementasyon Durumu (21/05/2026)

Bu bölüm, GDD v0.5 tasarım hedeflerini değiştirmez; mevcut Unity projesinde görülen üretim durumunu özetler.

### Kurulu Çekirdek Sistemler
- Grid ve Merge: `GridManager`, `GridTileView`, `GridTileData` ve `MergeableObject` ile 4 yönlü komşuluk üzerinden en az 3 aynı objeyi birleştiren temel merge akışı çalışır durumda.
- Kapalı Grid: Tile kilitleme/açma mantığı var. Kilitli tile üzerindeki obje sürüklenemiyor; merge grubu kilitli tile'a dokunduğunda tile açılıyor.
- Save/Load: `SaveManager` JSON save dosyasıyla base objelerini, locked tile listesini, inventory item ID'lerini, Time Credit'i ve Chrono Charge'ı saklıyor. Base sahnesi ile level sahnesi ayrımı yapılmış; level sahnesinde base snapshot'ı ezilmeden yalnızca inventory/economy verileri güncelleniyor.
- Base Akışı: `BaseManager`, base sahnesi açılırken grid kayıtlarının tamamlanmasını bekliyor, kayıtlı base state'ini yeniden kuruyor ve level ödüllerini ilk boş uygun tile'a yerleştiriyor.
- Level Akışı: `LevelManager`, hedef item üretildiğinde level tamamlanmasını, reward item'ın inventory'ye eklenmesini, opsiyonel Chrono Charge ödülünü ve win panelini yönetiyor.
- Market ve Time Credit: `MarketManager`, Time Credit ile item satın alma ve obje satma akışını yönetiyor. `TimeCreditUI` sahnede bağlı görünüyor.
- Chrono Charge: `GameSaveData`, `SaveManager`, `LevelSelector`, `LevelManager` ve `ChronoChargeUI` tarafında runtime/save/UI temeli var. BaseScene içinde Level_1 için Chrono cost yapılandırması görülüyor.
- Quest: `LevelQuestData`, `LevelQuestManager` ve `QuestUI_Item` ile `ProduceItem` tipi görev ilerlemesi var. `MergeCountLimit` enum'da tanımlı fakat davranış olarak uygulanmamış.
- Anomaly: `AnomalyManager` Type 1 random tile lock ve Type 2 orb tabanlı etkileri kod tarafında destekliyor. Level sahnelerinde `AnomalyManager` mevcut; en az Level_1'de Type 2 orb listesi ve foreign item havuzu boş olduğu için sistemin sahne konfigürasyonu tamamlanmamış.

### Aktif Sahne ve İçerik Durumu
- Build Settings içinde `MainMenuScene`, `BaseScene`, `Level_1`, `Level_2`, `Level_3` aktif.
- BaseScene içinde `BaseManager`, `SaveManager`, `LevelSelector`, `MarketManager`, `TimeCreditUI` ve `ChronoChargeUI` referansları var.
- Level_1, Level_2 ve Level_3 sahnelerinde `LevelManager`, `SaveManager` ve `AnomalyManager` bulunuyor.
- ScriptableObject tarafında test merge zinciri (`Tomurcuk_L1`, `CicekL2`, `BuyukCicek_L3`) ve quest assetleri var. Bu itemların `FacilityPoint`, `BuyPrice`, `SellPrice` değerleri şu an çoğunlukla 0 görünüyor.

### Eksik veya Yarım Kalan GDD Sistemleri
- Level progression/unlock sistemi henüz data-driven değil. `LevelData` ScriptableObject, completed level listesi ve save/load sonrası unlock korunumu yok.
- Facility Power ve base genişletme sistemi henüz manager/UI/threshold olarak uygulanmamış. Item data içinde `FacilityPoint` alanı var ama toplam hesaplama ve grid expansion yok.
- Quest sistemi level completion ile tam entegre değil; quest reward, zorunlu/opsiyonel quest ayrımı ve `MergeCountLimit` davranışı eksik.
- Anomaly sisteminde Type 2 sahne kurulumları, foreign item yaşam döngüsü ve save/load kapsam kararı netleşmemiş.
- Market unlock koşulları, item validation, icon polish ve yetersiz kaynak UI durumları ürün seviyesinde tamamlanmamış.
- Save şeması `SaveVersion` içeriyor; ancak completed level, unlocked expansion, quest progress ve anomaly state henüz save modelinde yok.
- Debug/QA paneli, soft-lock uyarısı ve base dolu durumda reward UI geri bildirimi henüz yok.

### Yakın Hedef
Bir sonraki mantıklı üretim adımı, Chrono Charge akışını sahnede manuel test edip kapatmak ve ardından `LevelData + CompletedLevelIds` ile level unlock/progression omurgasını kurmaktır. Bundan sonra Facility Power sistemi item data'daki mevcut `FacilityPoint` alanını kullanarak eklenebilir.
