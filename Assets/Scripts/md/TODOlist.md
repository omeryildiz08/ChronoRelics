# eMERGEncy TODO List

Bu dokuman, mevcut calisan sistemleri bozmadan oyunu GDD v0.5'te tarif edilen urun dongusune yaklastirmak icin hazirlandi.

Ana prensipler:
- GDD ana referanstir; tasklar GDD'deki core loop, ekonomi, base ve quest sistemlerini gorunur hale getirmeye hizmet eder.
- Mevcut calisan grid, merge, market, anomaly ve save/load akislarina dogrudan buyuk refactor uygulanmayacak; once ustune kucuk, test edilebilir sistemler eklenecek.
- Clean code ve SOLID icin yeni sorumluluklar ayri manager/service siniflarina bolunecek. `GridManager`, `SaveManager` ve `MarketManager` zaten yuksek sorumluluk tasiyor; yeni ozellikler bu siniflari daha da sisirmemeli.
- Her kod taskinin Unity Editor/sahne baglantisi ve placeholder gorunurlugu vardir. Kod calissa bile sahnede test edilemiyorsa task bitmis sayilmaz.
- Artist assetleri gecikirse placeholder UI, renkli materyal, basit prefab, TextMeshPro label ve primitive mesh ile sistem gorunur kilinacak.

## 0. Mevcut Durumu Korumak

### Kod
- [ ] Son 5 committe gelen save/load davranisini regression'a sokmamak icin kabul senaryolarini manuel test listesi olarak sabitle.
- [ ] `SaveManager`, `BaseManager`, `GridManager`, `MarketManager`, `AnomalyManager` icin kisa sorumluluk notlari ekle veya bu dokumanda referansla.
- [ ] Yeni sistemlerde once public API ekle, mevcut call site'lari minimum degistir.
- [ ] Sahne adlarini string literal olarak cogaltma; yeni ihtiyaclarda inspector field veya merkezi config kullan.
- [ ] Yeni economy/progression verilerini once save modeline eklemeden once runtime API olarak tasarla.

### Unity Editor
- [ ] BaseScene icin mevcut market, grid ve save akislarini bozmadan bir "Regression Test" checklist'i tut.
- [ ] Her yeni UI paneli icin prefab veya sahne objesi adi standart olsun: `ChronoChargeUI`, `FacilityPowerUI`, `DebugPanel` gibi.
- [ ] Placeholder assetler `PREFAB/Placeholder` veya benzeri tek bir klasorde toplanacak.

### Kabul Kriterleri
- [ ] Marketten alinan item level'e girip donunce korunuyor.
- [ ] Level reward base'e donunce ilk bos tile'a geliyor.
- [ ] Base'te merge sonrasi dogan item save/load sonrasi yerinde kaliyor.
- [ ] Obje satisi Time Credit'i artiriyor ve save/load sonrasi korunuyor.
- [ ] Locked tile merge'e katildiginda aciliyor.

## 1. Progression Omurgasi: Chrono Charge

GDD referansi: Chrono Charge, oyuncunun zaman/level degistirmek icin kullandigi kaynak.

### Kod
- [ ] `GameSaveData` icine `ChronoCharge` alani ekle.
- [ ] `SaveManager` icine `CurrentChronoCharge` runtime state'i ekle.
- [ ] `SaveManager` icine su API'leri ekle:
  - [ ] `AddChronoCharge(int amount, bool saveImmediately = true)`
  - [ ] `CanSpendChronoCharge(int amount)`
  - [ ] `SpendChronoCharge(int amount, bool saveImmediately = true)`
- [ ] Level gecis maliyeti icin `LevelSelector` icinde inspector'dan ayarlanabilir cost yapisi ekle.
- [ ] Basit ilk implementasyonda her level butonu veya scene entry icin `ChronoCost` kullan.
- [ ] Yetersiz Chrono Charge durumunda sahne yuklenmesin, feedback verilsin.
- [ ] Level tamamlaninca opsiyonel Chrono Charge odulu verilebilsin.
- [ ] Time Credit ile Chrono Charge'i karistirmamak icin economy API isimlerini net tut.

### Unity Editor / UI
- [ ] BaseScene'e `ChronoChargeUI` TextMeshPro label ekle.
- [ ] Level selection panelinde her level icin Chrono cost goster.
- [ ] Yetersiz Chrono durumunda placeholder feedback text'i veya toast kullan.
- [ ] Chrono icon gelene kadar basit saat/enerji placeholder sprite'i kullan.
- [ ] Test icin inspector'dan baslangic Chrono degeri verilebilecek debug alan veya buton ekle.

### Kabul Kriterleri
- [ ] Chrono Charge save/load sonrasi korunuyor.
- [ ] Yetersiz kaynakla level yuklenmiyor.
- [ ] Yeterli kaynakla level yukleniyor ve kaynak dusuyor.
- [ ] Level tamamlaninca belirlenen Chrono odulu ekleniyor.

## 2. Level Progression ve Unlock Sistemi

GDD referansi: oyuncu farkli zaman dilimlerine gider, level tamamlayarak ilerler.

### Kod
- [ ] Level bilgisi icin data yapisi tasarla: `LevelId`, `SceneName`, `ChronoCost`, `RequiredPreviousLevel`, `RewardItem`, `ChronoReward`.
- [ ] Basit cozum icin ScriptableObject tabanli `LevelData` ekle.
- [ ] `GameSaveData` icine tamamlanan level ID listesi ekle.
- [ ] `SaveManager` icine `MarkLevelCompleted(string levelId)` ve `IsLevelCompleted(string levelId)` ekle.
- [ ] `LevelManager` hedef item tamamlandiginda level'i completed olarak isaretlesin.
- [ ] `LevelSelector` locked/unlocked kontrolu yapsin.
- [ ] Direkt scene name ile yukleme devam edebilir ama LevelData uzerinden yonlendirilmeli.

### Unity Editor / UI
- [ ] Level selection panelinde locked level butonlari disable veya kilit ikonlu goster.
- [ ] Placeholder level iconlari kullan.
- [ ] Tamamlanan level icin checkmark veya renk farki ekle.
- [ ] Level butonlarina `LevelData` referansi bagla.

### Kabul Kriterleri
- [ ] Ilk level acik, sonraki level kilitli basliyor.
- [ ] Level tamamlaninca sonraki level aciliyor.
- [ ] Save/load sonrasi unlock durumu korunuyor.

## 3. Facility Power ve Base Genisletme

GDD referansi: base'e yerlestirilen objeler Facility Power uretir; threshold gecilince daha fazla grid alani acilir.

### Kod
- [ ] `MergeableItemData.FacilityPoint` mevcut alanini kullanarak toplam base power hesaplayan `FacilityPowerManager` ekle.
- [ ] `GridManager` icine power hesaplama koyma; grid sadece tile/object bilgisini saglamali.
- [ ] `FacilityPowerManager` base sahnesinde grid snapshot'ini okuyup toplam power hesaplasin.
- [ ] Threshold data yapisi ekle: `RequiredPower`, `UnlockedTileGroupId` veya `ExpansionLevel`.
- [ ] Ilk asamada fiziksel grid instantiate etmek yerine var olan tile'lari locked/hidden durumdan ac.
- [ ] `GameSaveData` icine `UnlockedExpansionLevel` veya unlocked tile grup bilgisi ekle.
- [ ] Base'te item al, sat, merge, move sonrasi Facility Power recalculation tetiklensin.
- [ ] Hesaplama event tabanli olsun; gerekirse `GridManager.OnBaseStateChanged` gibi genel bir event ekle.

### Unity Editor / UI
- [ ] BaseScene'e `FacilityPowerUI` ekle: `43 / 45 FP` gibi.
- [ ] Acilacak grid bolgelerini sahnede placeholder locked material ile goster.
- [ ] Expansion threshold gecilince tile material veya visibility degisimi gorunur olsun.
- [ ] Facility icon gelene kadar basit enerji/binacik placeholder icon kullan.

### Kabul Kriterleri
- [ ] Base'teki objelerden toplam Facility Power dogru hesaplaniyor.
- [ ] Obje satilinca power dusuyor, obje alinca/merge olunca power guncelleniyor.
- [ ] Threshold gecilince yeni tile grubu aciliyor.
- [ ] Save/load sonrasi acilan gridler korunuyor.

## 4. Quest Sistemini Genisletme

GDD referansi: level icinde ana ve yan objective'ler; base'te oyuncuyu yonlendiren gorevler.

### Kod
- [ ] Mevcut `ProduceItem` quest davranisini koru.
- [ ] `MergeCountLimit` enum'da var ama uygulanmamis; handler ekle.
- [ ] Quest progress state'ini UI component icinden ayir. `QuestUI_Item` sadece gorunum yonetsin.
- [ ] `LevelQuestManager` icinde quest runtime state listesi tut.
- [ ] Quest tamamlaninca event firlat: `OnQuestCompleted`.
- [ ] Quest odul data alanlari ekle:
  - [ ] Time Credit reward
  - [ ] Chrono Charge reward
  - [ ] Item reward
- [ ] Level tamamlanma kosulu icin opsiyon ekle:
  - [ ] Target item uretildi
  - [ ] Ana quest tamamlandi
  - [ ] Tum zorunlu questler tamamlandi
- [ ] Base questleri icin daha sonra ayri `BaseQuestManager` dusun; level quest kodunu sisirme.

### Unity Editor / UI
- [ ] Quest UI'da tamamlanmis, aktif ve basarisiz durumlari gorunur yap.
- [ ] `MergeCountLimit` icin kalan merge sayisi text'i ekle.
- [ ] Quest odulu placeholder icon/text olarak goster.
- [ ] Quest complete icin artist VFX beklenmeden checkmark + basit particle kullan.

### Kabul Kriterleri
- [ ] ProduceItem quest eskisi gibi calisir.
- [ ] MergeCountLimit quest merge sayisini takip eder.
- [ ] Quest tamamlaninca odul verir.
- [ ] Level tamamlanma kosulu quest sistemine baglanabilir.

## 5. Anomaly Sistemini Urune Hazirlama

GDD referansi: merge sonrasi random anomaly, orb anomaly ve 3x3 etki alani.

### Kod
- [ ] Anomaly sisteminin base'te kalici mi level'a ozel gecici mi olacagina karar ver.
- [ ] Eger kalici olacaksa save modeline ekle:
  - [ ] Type1 locked anomaly tile pozisyonlari
  - [ ] Type2 orb pozisyonlari
  - [ ] Inactive anomaly item state'i
- [ ] `IsInactiveAnomalyItem` sadece runtime flag kalirsa save/load sonrasi kaybolur; save'e dahil et veya level-only yap.
- [ ] Anomaly spawn edilen foreign item'in oyuncuya ne zaman temizlenecegini tasarla.
- [ ] `AnomalyManager` random secimleri icin debug override'lari koru ama release'de kapali baslat.
- [ ] Type2 effect weight'lerini ScriptableObject config'e tasimayi degerlendir.

### Unity Editor / UI
- [ ] Orb icin placeholder sphere prefab ekle.
- [ ] Type1 rift icin basit renkli particle veya emissive plane kullan.
- [ ] Inactive anomaly item icin ayri material tint veya outline kullan.
- [ ] Debug icin anomaly tetikleme butonlari ekle.

### Kabul Kriterleri
- [ ] Type1 anomaly merge sonrasi tile kilitleyebiliyor.
- [ ] Type2 orb 3x3 etki alaninda calisiyor.
- [ ] Inactive anomaly item suruklenemiyor ve satilamiyor.
- [ ] Save/load karari uygulanan kapsama gore tutarli.

## 6. Anomaly Market ve Ekonomi Polish

GDD referansi: oyuncu istemedigi objeleri Time Credit karsiliginda satar, marketten dusuk seviye objeler alir.

### Kod
- [ ] Mevcut `MarketManager.TryBuyItem` ve `TrySellObject` davranisini koru.
- [ ] Market item listesinde unlock kosulu ekle: level tamamlandi, base power, story progress gibi.
- [ ] `MergeableItemData` icindeki `BuyPrice` ve `SellPrice` alanlari icin validation ekle.
- [ ] Satilamaz itemler icin data flag eklemeyi degerlendir: `CanSell`.
- [ ] Market refresh icin manuel `BuildMarketUI` yerine state degisince refresh event'i ekle.
- [ ] Time Credit harcamalari ve kazanimlari tek API uzerinden gecmeye devam etsin.

### Unity Editor / UI
- [ ] Market item prefabinda icon alanini aktif kullan.
- [ ] Icon yoksa item level/name ile placeholder kart tasarimi yap.
- [ ] Buy button yetersiz Time Credit durumunda disabled veya feedback'li olsun.
- [ ] Drag & drop sell zone gorunurlugunu daha belirgin yap.
- [ ] Locked tile uzerindeki obje suruklenemedigi icin satisa da gidemeyecegini UI feedback ile anlat.

### Kabul Kriterleri
- [ ] Market sadece unlocked itemleri gosterir.
- [ ] Satin alma ve satis save/load sonrasi korunur.
- [ ] Yetersiz Time Credit oyuncuya net gosterilir.

## 7. Save/Load Sema Genisletme

Mevcut save sistemi son commitlerde duzeltildi; genisletirken ayni ayrimi koru: base snapshot level sahnesinden ezilmemeli.

### Kod
- [ ] `GameSaveData` icin sema versiyonu ekle: `SaveVersion`.
- [ ] Yeni alanlar:
  - [ ] `ChronoCharge`
  - [ ] `CompletedLevelIds`
  - [ ] `UnlockedExpansionLevel`
  - [ ] `QuestProgress`, gerekiyorsa
  - [ ] `AnomalyState`, karara bagli
- [ ] Legacy save dosyalari icin default degerler belirle.
- [ ] `SaveInventoryOnly` yeni economy/progression alanlarini da koruyarak yazsin.
- [ ] Save loglarini debug flag'e bagla; su anki detay loglar development icin faydali ama release'de gurultu yaratir.
- [ ] Save batching/debounce dusun: base'te cok sik move yapilinca her hareket dosyaya yaziyor.

### Unity Editor / Test
- [ ] Delete save context menu kullanimi dokumante edilsin.
- [ ] Yeni save alanlari icin manuel test senaryolari yaz.
- [ ] Eski save dosyasi ile oyunu acip migration/default davranisini kontrol et.

### Kabul Kriterleri
- [ ] Eski save crash yaratmadan aciliyor.
- [ ] Yeni kaynaklar save/load sonrasi korunuyor.
- [ ] Level sahnesinde save almak base objelerini ezmiyor.

## 8. Main Base Akisini Guclendirme

GDD referansi: level sonunda kazanilan objeler base'e gelir, base'te merge ve hikaye ilerlemesi surer.

### Kod
- [ ] Reward inventory sistemi korunacak; base rebuild sonrasi inventory spawn akisi devam edecek.
- [ ] Inventory spawn grid doluysa item kaybolmasin; inventory'de kalsin ve UI feedback versin.
- [ ] Base grid doluluk kontrolu icin helper ekle: `HasEmptyUnlockedTile`.
- [ ] Soft-lock'e yakin durumda uyarici sistem ekle.
- [ ] Base'te merge olan itemlarin story/progression tetiklemesi icin event hook ekle.

### Unity Editor / UI
- [ ] Grid doluysa ekranda "Base dolu" feedback'i goster.
- [ ] Inventory'de bekleyen reward varsa ufak bir panel/listede goster.
- [ ] Base hedefleri icin placeholder quest/task paneli ekle.

### Kabul Kriterleri
- [ ] Grid doluyken reward kaybolmaz.
- [ ] Oyuncu base'te ne yapmasi gerektigini UI'dan anlayabilir.
- [ ] Base merge'i progression event'i tetikleyebilir.

## 9. UI ve Placeholder Asset Stratejisi

Artist assetleri son urun olarak gelmese bile sistemler sahnede okunabilir olmali.

### UI
- [ ] Top resource bar: Time Credit, Chrono Charge, Facility Power.
- [ ] Level selection panel: level status, Chrono cost, reward preview.
- [ ] Market panel: item name, level, buy price, sell feedback.
- [ ] Quest panel: icon, description, progress, reward.
- [ ] Debug panel: development build icin toggle edilebilir.

### Placeholder Asset
- [ ] Chrono Charge icon: basit saat/enerji sprite.
- [ ] Time Credit icon: coin/credit placeholder.
- [ ] Facility Power icon: lightning/building placeholder.
- [ ] Locked tile material: koyu/kapali renk.
- [ ] Unlockable tile material: yari saydam veya outline.
- [ ] Anomaly orb: emissive sphere.
- [ ] Rift effect: particle veya renkli plane.
- [ ] Missing item icon: item level text'li generic sprite.

### Kabul Kriterleri
- [ ] Her yeni sistem UI'da gorunur.
- [ ] Placeholder ile final asset kolayca degistirilebilir.
- [ ] UI baglantilari inspector'da acik ve isimlendirme tutarli.

## 10. Debug Panel ve QA Araclari

### Kod
- [ ] `DebugGamePanel` veya `DeveloperTools` sinifi ekle.
- [ ] Development build veya inspector bool ile aktif olsun.
- [ ] Butonlar:
  - [ ] +100 Time Credit
  - [ ] +5 Chrono Charge
  - [ ] Spawn selected item
  - [ ] Lock selected/random tile
  - [ ] Unlock all tiles
  - [ ] Trigger Type1 anomaly
  - [ ] Trigger Type2 anomaly
  - [ ] Complete current level
  - [ ] Delete save
- [ ] Debug fonksiyonlari production flow'u bypass ettigini loglasin.

### Unity Editor
- [ ] BaseScene'e debug panel prefab'i ekle ama default kapali tut.
- [ ] Level sahnelerine minimal debug panel veya ayni prefab ekle.
- [ ] Debug panel butonlari TextMeshPro ile net etiketlensin.

### Kabul Kriterleri
- [ ] Tek sahnede economy/progression/anomaly test edilebilir.
- [ ] Debug panel kapaliyken normal oyuncu akisina etki etmez.

## 11. Kod Kalitesi ve Mimari Temizlik

Bu kisim feature eklerken paralel ilerlemeli; buyuk refactor olarak tek seferde yapilmamali.

### Kod
- [ ] `GridManager` icinde save cagrisini azaltmak icin genel state changed event'i dusun.
- [ ] `SaveManager` file IO, runtime state ve lookup sorumluluklarini uzun vadede bolmeyi planla.
- [ ] `QuestUI_Item` icindeki progress state'i manager'a tasinmali.
- [ ] `MarketManager` hem UI hem ekonomi yapiyor; ileride economy service ayrilabilir.
- [ ] Singleton kullanimlari devam edebilir ama yeni sistemlerde null guard ve inspector fallback net olsun.
- [ ] Public fieldlar icin `[Header]`, `[Tooltip]`, `[SerializeField] private` tercih edilmeli.
- [ ] Magic stringleri azalt: scene name, item ID, level ID.
- [ ] Yeni event isimleri domain odakli olsun: `OnChronoChargeChanged`, `OnFacilityPowerChanged`.
- [ ] Her manager icin sorumluluk siniri belirle:
  - [ ] Grid: tile/object state
  - [ ] Save: persistence
  - [ ] Economy: currency operations
  - [ ] Progression: level unlock/completion
  - [ ] Quest: objective state
  - [ ] UI: sadece gorunum ve input

### Kabul Kriterleri
- [ ] Yeni feature eklemek icin mevcut calisan methodlari buyuk olcude degistirmek gerekmiyor.
- [ ] Her yeni sistem inspector'dan test edilebilir.
- [ ] Kod okunabilirligi feature hizina feda edilmiyor.

## 12. Onerilen Implementasyon Sirasi

1. [ ] Chrono Charge save/runtime/UI.
2. [ ] LevelData + level unlock/progression.
3. [ ] FacilityPowerManager + UI + expansion placeholder.
4. [ ] Quest runtime state + reward + MergeCountLimit.
5. [ ] Save sema versiyonu ve yeni alanlarin tam entegrasyonu.
6. [ ] Anomaly state karari ve stabilizasyon.
7. [ ] Market unlock/polish.
8. [ ] Base soft-lock/reward inventory UI.
9. [ ] Debug panel.
10. [ ] Log azaltma, validation ve regression checklist.

## 13. Manuel Regression Checklist

Her buyuk tasktan sonra calistirilacak temel testler:

- [ ] Yeni save ile BaseScene aciliyor.
- [ ] Marketten item aliniyor, Time Credit dusuyor.
- [ ] Item satiliyor, Time Credit artiyor.
- [ ] Level'e girip base'e donunce base objeleri korunuyor.
- [ ] Level reward base'e ekleniyor.
- [ ] Base'te merge yapiliyor, yeni item save/load sonrasi korunuyor.
- [ ] Locked tile uzerindeki item suruklenemiyor.
- [ ] Locked tile merge'e katilinca aciliyor.
- [ ] Anomaly tetiklenince oyun soft-lock/crash yaratmiyor.
- [ ] Save silinince temiz baslangic yapiliyor.

