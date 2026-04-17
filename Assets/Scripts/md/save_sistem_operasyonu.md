# Save Sistem Operasyonu

## Amac

Bu dokumanin amaci, base <-> level gecislerinde save/load davranisini tutarli hale getirmek icin yapilan analizi, uygulanan fix'i ve sistemin yeni calisma kurallarini netlestirmektir.

Hedef davranis:

- Base'te mevcut objeler level'e gidip donuldugunde ayni konumlarda kalmali.
- Level odulu olarak gelen objeler base'e eklendiginde kaybolmamali.
- Markette satin alinan objeler level donusunde kaybolmamali.
- Base'te mevcut merge kurallari neyse onlar gecerli kalmali; save sistemi yeni bir merge feature'u eklememeli.
- Base'te bir merge oldugunda, eski objeler yerine merge'den dogan yeni obje ve onun grid konumu kalici state olarak tutulmali.
- Satis, satin alma, merge, tile lock ve level odulu akisleri ayni save modeline hizmet etmeli.

## Kisa Sonuc

Bug artik cozulmus durumda. Marketten alinan item'lar, level odulleri ve base icinde merge sonucu dogan item'lar level'e gidip donuldugunde korunuyor.

Kok neden save formatinin bozuk olmasi degildi. Asil problem initialization sirasi ve restore akisinin grid tam hazir olmadan calismasiydi.

## Kok Neden

Ilk bakista sorun sunlardan biri gibi gorunuyordu:

- market item'lari save'e hic yazilmiyor olabilir
- `ItemID` lookup bozuluyor olabilir
- merge sonrasi yeni obje grid'e dogru register edilmiyor olabilir

Loglar geldikten sonra asil resim netlesti:

1. `SaveManager.LoadGame()` kayitli `SavedObjects` verisini dogru okuyordu.
2. Buna ragmen base sahnesi acildiginda bazen sadece reward item sahnede kaliyor, market item'lari veya merge sonucu item'lar kayboluyordu.
3. Reward spawn'in zaman zaman `(5, 0)` gibi anlamsiz gec bir bos tile'a gitmesi, grid'deki erken tile'larin henuz register edilmedigini gosteriyordu.
4. Yani save dosyasinda veri vardi ama `BaseManager.Start()` restore akisini, `GridTileView`'larin `GridManager` icine tam kaydi bitmeden baslatiyordu.

Sonuc:

- Kayitli objeler restore edilmek istendiginde bazi tile'lar henuz hazir degildi.
- Restore akisi eksik veya yarim kaliyordu.
- Ardindan inventory odulu spawn olup tekrar `SaveGame()` cagirinca, yari restore edilmis state eziliyordu.
- Bu da "yalnizca odul objesi kaliyor" veya "merge sonucu obje gidiyor" gibi semptomlar uretiyordu.

Bu bug'in ozeti su:

- veri dogruydu
- zamanlama yanlisti

## Uygulanan Fix

Fix uc ana eksende yapildi:

1. Save modeli ayrildi ve runtime state netlestirildi.
2. Base restore akisi deterministic hale getirildi.
3. Base uzerindeki tum kritik mutasyonlar standart save noktalariyla kalici hale getirildi.

## Degisen Kod Bloklari Ve Nedenleri

### 1. `Assets/Scripts/SaveSys/GameSaveData.cs`

Bu dosyada save modelini ayirdik.

Eklenen alanlar:

- `List<SavedObjectData> SavedObjects`
- `List<LockedTileSaveData> LockedTiles`
- legacy uyumluluk icin `SavedTiles` korunmaya devam etti

Neden degisti:

- Eski modelde `SavedTiles` hem obje state'ini hem lock state'ini tasiyordu.
- Bu iki sorumluluk birbirine karisiyordu.
- Yeni modelle kalici objeler ile locked tile bilgisi birbirinden ayrildi.

Bu sayede:

- bos ama locked tile
- dolu tile
- inventory'de bekleyen odul

birbirine karismadan saklanabiliyor.

### 2. `Assets/Scripts/SaveSys/SaveManager.cs`

En buyuk degisiklik burada yapildi.

Eklenen veya degisen ana bloklar:

- `CurrentSavedObjects`
- `CurrentLockedTiles`
- `RefreshItemLookup()`
- `CollectSavedObjects(GridManager)`
- `CollectLockedTiles(GridManager)`
- `GetSavedObjectsForLoad(GameSaveData)`
- `GetLockedTilesForLoad(GameSaveData)`
- `RebuildBaseScene()`
- `TryReadSaveData()`, `WriteSaveData()`
- detayli debug loglari

Neden degisti:

- Save tarafinda artik sadece inventory ve para degil, base'in kalici obje snapshot'i da runtime'da tutuluyor.
- `SaveGame()` base sahnesindeyse `SavedObjects` ve `LockedTiles` uretip diske yaziyor.
- Level sahnesindeyse `SaveInventoryOnly()` calisiyor; yani base snapshot'i ezilmiyor, sadece inventory ve time credit guncelleniyor.

Bu kritik ayrim su problemi cozmektedir:

- Level tamamlandiginda odul envantere eklenir.
- O sirada base sahnesi aktif olmadigi icin base objeleri tekrar toplanmaya calisilmaz.
- Boylece level sahnesi, base snapshot'ini yanlislikla sifirlayamaz.

`RebuildBaseScene()` neden eklendi:

- `LoadGame()` artik sadece veriyi okur ve runtime state'e alir.
- Sahneye obje basma isi ayri bir adim olarak `RebuildBaseScene()` icine alinmistir.
- Bu separation of concerns sayesinde "dosyayi okumak" ile "sahneyi rebuild etmek" birbirinden ayrildi.

Bu, bug'i cozerken cok onemliydi cunku artik rebuild'in ne zaman cagrilacagini biz kontrollu sekilde belirleyebiliyoruz.

### 3. `Assets/Scripts/BaseManager.cs`

Buradaki degisiklik bug'in ana fix'idir.

Eski durum:

- `Start()` icinde dogrudan:
  - `LoadGame()`
  - `CheckInventoryAndSpawn()`

Yeni durum:

- `Start()` coroutine oldu
- once `WaitForGridInitialization()` calisiyor
- sonra:
  - `SaveManager.Instance.LoadGame()`
  - `SaveManager.Instance.RebuildBaseScene()`
  - `CheckInventoryAndSpawn()`

`WaitForGridInitialization()` neden eklendi:

- `GridTileView`'larin hepsi `GridManager` icine register olmadan restore baslamasin diye
- sahnedeki beklenen tile sayisi ile `GridManager.CountRegisteredTiles()` karsilastiriliyor
- butun tile'lar hazir olduktan sonra restore akisi baslatiliyor

Bu degisiklik su siralamayi garanti etti:

1. Grid hazir
2. Kayitli base objeleri geri kuruluyor
3. Inventory'deki yeni oduller ilk bos yerlere ekleniyor
4. Yeni birlesik state tekrar save ediliyor

Bu siralama dogrudan bug'i cozen davranistir.

### 4. `Assets/Scripts/Grid/GridTileView.cs`

Bu dosyada tile registration zamani degisti.

Eski durum:

- tile registration sadece `Start()` icinde yapiliyordu

Yeni durum:

- `Awake()` icinde `TryRegisterTile()`
- `Start()` icinde bir kez daha `TryRegisterTile()`
- `isRegistered` guard'i ile cift registration engelleniyor

Neden degisti:

- `GridManager` hazirsa tile olabildigince erken register edilsin
- herhangi bir Unity lifecycle race durumunda `Start()` ikinci emniyet noktasi olsun
- ama ayni tile iki kez register edilmeye calisildiginda sistem bozulmasin

Bu degisiklik tek basina yeterli degildi, ama `BaseManager` coroutine fix'i ile beraber bug'i kapatti.

### 5. `Assets/Scripts/Grid/GridManager.cs`

Burada iki tur degisiklik yapildi.

Eklenen yardimci metot:

- `CountRegisteredTiles()`

Neden eklendi:

- `BaseManager.WaitForGridInitialization()` icin grid'in gercekten hazir olup olmadigini sayisal olarak kontrol etmek gerekiyordu

Eklenen save noktasi standardizasyonu:

- `TryMergeOrPlace()` sonunda
- `PerformMerge()` sonunda
- `ForceMoveObject()` sonunda
- `RemoveObject()` sonunda
- ortak yardimci olarak `SaveBaseStateIfNeeded()`

Neden degisti:

- Base uzerinde state degistiren her aksiyon sonrasinda kalici snapshot guncellenmeli
- merge sonucu yeni obje olusursa bu obje hemen save'e dusmeli
- obje satildiginda veya tasindiginda base snapshot gecikmeli degil, anlik olarak guncellenmeli

Bu sayede artik:

- market satisi
- market alimi
- base icinde surukleme/tasima
- merge sonucu yeni obje olusmasi

hepsi ayni persistence hattina bagli.

## Neden Bu Fix Calisti

Bir junior'a anlatir gibi en sade haliyle:

Sistemde iki farkli problem tipi vardir:

1. Yanlis veri kaydetmek
2. Dogru veriyi yanlis zamanda uygulamak

Bizde ikinci problem vardi.

Save dosyasi dogru veriyi tutabiliyordu. Ama base sahnesi acildiginda restore fazla erken calisiyordu. Grid henuz eksik oldugu icin restore edilen objelerin bir kismi sahneye oturmuyordu. Sonra reward spawn gelip yeni save aldiginda, eksik restore edilmis state kalici hale geliyordu.

Fix sonrasi su garanti altina alindi:

- once altyapi hazirlanir
- sonra mevcut base state geri kurulur
- en son yeni oduller eklenir

Bu bir init-order bug fix'idir.

## Yeni Sistem Invariant'lari

Sistemin bundan sonra dogru calistigini varsaymak icin su kurallar her zaman korunmali:

- Base sahnesinde kalici state'in kaynagi `SavedObjects` + `LockedTiles` snapshot'idir.
- Level sahnesinde `SaveGame()` tam base snapshot almaz; sadece inventory/time credit gunceller.
- Base restore akisi grid tile registration tamamlanmadan baslamaz.
- Inventory odulleri, mevcut base state restore edildikten sonra eklenir.
- Merge sonrasi dogan yeni obje bir sonraki level gecisini beklemeden save'e yazilir.
- Marketten alinan obje inventory'ye degil, dogrudan base state'e girer.
- Base'te sahneye yerlestirilen odul inventory'den cikar ve artik `SavedObjects` icinde yasar.

## Loglardan Gozlenen Saglikli Davranis

Fix sonrasi loglarda su paternler gorulmeye baslandi:

- reward spawn artik `(5, 0)` gibi gec ve anlamsiz bosluklara gitmiyor; gercek ilk bos slotlara gidiyor
- `LOAD -> REBUILD -> REBUILD-SPAWN -> SPAWN Basarili` zinciri eksiksiz akiyor
- marketten alinan `hancerlvl1` load'da tekrar ayni pozisyonlarda kuruluyor
- merge sonrasi `baltalvl2` save'e yaziliyor ve sonraki load'da ayni pozisyonda geri geliyor
- level tamamlandiginda `Sadece envanter guncellendi (level modu). SavedObjects korunuyor: X` logu goruluyor; yani base snapshot level tarafinda ezilmiyor

Bu log davranisi, fix'in sadece "gorsel olarak sans eseri calismadigini", gercekten veri akisinin duzeldigini gosteriyor.

## Kabul Kriterleri

Yeni sistem asagidaki senaryolarda tutarli calismalidir:

1. Base'te marketten `Gurz` veya `Hancer` satin al, level'e gir, don.
Beklenen: Satin alinan obje ayni yerde durmali.

2. Level bitirip `Mizrak` odulu al, base'e don.
Beklenen: Mevcut objeler korunmali, yeni `Mizrak` ilk bos tile'a eklenmeli.

3. Base'te odul objesi ile marketten alinmis objeleri mevcut merge kurallariyla birlestir.
Beklenen: Eski objeler silinmeli, merge'den dogan yeni obje dogru yerde olusmali ve save'e girmeli.

4. Merge sonrasi yeni objeyi sat, yerine marketten obje al, tekrar level'e girip don.
Beklenen: Para, yeni obje ve kalan base state tamamen korunmali.

5. Oyunu kapatip yeniden ac.
Beklenen: Base'teki objeler ve lock state en son kaydedilen snapshot ile geri gelmeli.

## Ileride Temizlenebilecek Noktalar

Su anki log seviyesi debug icin bilerek yuksek tutuldu. Sistem stabil kaldigi gorulurse ileride:

- `DebugSavedObjects(...)` loglari azaltilabilir
- `BaseManager` ve `SaveManager` icindeki detayli spawn loglari sadeleştirilebilir
- cok sik mutasyonlarda save batching/debounce dusunulebilir

Bunlar correctness fix'i degil, temizlik ve performans iyilestirmesidir.

## Sonuc

Bu operasyon sonunda save sistemi daha saglam hale geldi cunku:

- veri modeli daha net ayrildi
- restore zamani deterministic yapildi
- base mutasyonlari standart save noktalarina baglandi

En kritik teknik ders su:

Bir persistence bug'inda sadece "ne kaydediliyor?" sorusuna degil, "ne zaman restore ediliyor?" sorusuna da bakmak gerekir. Bu bug'in asil sebebi veri formatindan cok lifecycle sirasiydi. Fix de tam olarak o sirayi dogru hale getirerek yapildi.
