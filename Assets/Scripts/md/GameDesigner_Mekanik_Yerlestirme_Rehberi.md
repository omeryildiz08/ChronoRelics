# eMERGEncy - Game Designer Mekanik Yerleştirme Rehberi

Bu doküman, GDD v0.5 ve mevcut Unity kodları baz alınarak hazırlanmıştır. Amaç, level/base tasarımı yaparken hangi mekaniklerin nasıl çalıştığını, editörde hangi alanların ayarlandığını ve yerleşim kararlarının oyuna nasıl yansıdığını tek yerde toplamaktır.

> Kod durumu referansı: `Assets/Scripts` incelemesi, 09.06.2026.

## 1. Tasarım Mantığı Kısa Özet

eMERGEncy şu an şu çekirdek sistemlerle çalışır:

- Grid üstünde obje sürükleme, boş tile'a taşıma ve aynı itemlardan bağlı grup merge etme.
- Kilitli tile: oyuncu üstündeki objeyi sürükleyemez, kilitli tile'a obje bırakamaz. Merge grubu kilitli tile'daki objeyi kullanırsa tile açılır.
- Level hedefi: `LevelManager.TargetItem` üretilince level tamamlanır.
- Level ödülü: `RewardItem` inventory'ye eklenir, BaseScene açılınca ilk boş uygun tile'a otomatik yerleştirilir.
- Time Credit: base'de obje satışıyla kazanılır, marketten item almak için harcanır.
- Chrono Charge: level seçerken giriş maliyeti olarak harcanır, bazı levellardan ödül olarak verilebilir.
- Facility Power: base'deki objelerin FP toplamı ile kilitli base tile'ları manuel açılır.
- Anomaly Type 1: merge sonrası şansa bağlı rastgele bir tile'ı kilitler.
- Anomaly Type 2: sahneye yerleştirilen anomaly orb tile'ları merge sonrası çevresindeki 3x3 alanda rastgele etkiler üretir.
- Quest: şu an sadece belirli item üretimini sayan UI görevleri çalışır; level tamamlanma koşuluna bağlanmış değildir.

## 2. Merge ve Grid Sistemi

### Çalışma Kuralı

Oyuncu bir `MergeableObject` objesini sürükler ve bir tile'a bırakır. `GridManager.TryMergeOrPlace` bırakılan tile'a göre karar verir:

- Hedef tile geçersizse, kayıtlı değilse veya kilitliyse obje eski yerine döner.
- Hedef tile boşsa obje oraya taşınır.
- Hedef tile'da aynı `ItemData` olan başka obje varsa, o noktadan başlayan bağlı grup aranır.
- Bağlı aynı item sayısı 3 veya daha fazlaysa merge çalışır.
- Grup arama 4 yönlüdür: yukarı, aşağı, sol, sağ. Çapraz komşuluk sayılmaz.
- 3'ten fazla bağlı aynı obje varsa hepsi tek merge'e dahil olur.
- Yeni obje, merge'in bırakıldığı merkez tile'da oluşur.
- Yeni obje için kullanılan data, merge edilen itemın `NextLevelItem` alanıdır.

### Level Design Etkisi

- Oyuncunun merge yapabilmesi için aynı itemların 4-yönlü bağlı hale getirilebilmesi gerekir.
- Çapraz dizilimler bilinçli olarak merge vermez; bu, puzzle zorluğu için kullanılabilir.
- Çoklu bağlı grup davranışı nedeniyle 4 veya 5 aynı item yanlışlıkla tek merge'de yok olabilir. Dar alanda item yığarken bunu hesaba kat.
- Bir item zincirinin son itemında `NextLevelItem` boşsa o item merge edilemez; hedef olarak kullanılabilir ama daha üst aşamaya çıkamaz.

### Editör Kurulumu

Her grid tile objesinde `GridTileView` bulunmalı:

- `GridPosition`: tile'ın grid koordinatı. Dünya pozisyonu ile uyumlu olmalı.
- `objectYOffset`: objenin tile üstünde duracağı yükseklik.
- `StartLocked`: sahne başında kilitli başlayıp başlamayacağı.
- `NormalMaterial`, `LockedMaterial`, `UnlockableMaterial`: normal, kilitli ve açılabilir görünüm.
- Highlight materyalleri: sürükleme sırasında geçerli/geçersiz/merge adayı geri bildirimi.

Her oynanabilir item prefabında `MergeableObject` bulunmalı:

- `ItemData`: ilgili `MergeableItemData` ScriptableObject.
- Prefab üzerinde collider olmalı; sürükleme `OnMouseDown/OnMouseDrag/OnMouseUp` ile çalışıyor.
- `IsInactiveAnomalyItem` normal itemlarda kapalı kalmalı. Bu alan Type 2 foreign spawn tarafından runtime'da açılır.

Sahnede bir `GridManager` bulunmalı:

- `gridWidth`, `gridHeight`: tüm tile koordinatlarını kapsamalı.
- `mergeAnimationController`: varsa merge animasyonlarını oynatır.
- `mergeVFXPrefab`, `mergeSoundClip`, `errorSoundClip`, `audioSource`: feedback ayarları.

## 3. MergeableItemData ScriptableObject Rehberi

Item verileri `Create > Merge Game > Mergeable Item` ile oluşturulur.

Alanlar:

- `ItemID`: save/load için benzersiz ID. Boş veya tekrar eden ID kritik hata üretir.
- `ItemName`: UI'da gösterilecek isim.
- `Level`: tasarım seviyesi. Kod merge kararında bunu kullanmıyor; okunabilirlik ve dengeleme için kullanılır.
- `Prefab`: sahnede spawn edilecek prefab. Prefab içinde `MergeableObject` olmalı.
- `NextLevelItem`: bu item merge edilince oluşacak item.
- `SellPrice`: base market satış değeri. 0 veya altı ise satılamaz.
- `BuyPrice`: market alış değeri. 0 veya altı ise market listesinde görünmez.
- `FacilityPoint`: base'de bu itemın verdiği FP. Negatif değerler runtime'da 0 sayılır.

Tasarım önerisi:

- Her çağ/tema için ayrı merge zinciri oluştur: L1 -> L2 -> L3 -> hedef.
- Level hedefi olan itemın üst zinciri gerekmiyorsa `NextLevelItem` boş kalabilir.
- Marketten alınabilir itemlar için `BuyPrice > 0`, satılabilir itemlar için `SellPrice > 0` ver.
- Base genişletmede kullanılacak itemlara anlamlı `FacilityPoint` ver. Level itemlarının FP değeri 0 ise base progression'a katkı yapmaz.

## 4. Kapalı Grid / Kilitli Tile

### Çalışma Kuralı

Kilitli tile şu davranışlara sahiptir:

- Üzerindeki obje oyuncu tarafından sürüklenemez.
- Dışarıdan kilitli tile'a obje bırakılamaz.
- Eğer kilitli tile'daki obje, bir merge grubunun parçası olursa merge sırasında tile açılır.
- Kilit açılınca `Anomaly Type 1` efektleri de temizlenir.

### Editör Kurulumu

Bir tile'ın sahne başında kilitli olması için:

1. Tile objesindeki `GridTileView.StartLocked` açılır.
2. `LockedMaterial` atanır.
3. Eğer base expansion tile'ı olacaksa ayrıca `FacilityPowerManager.expansionGroups` içine eklenir.

### Level Design Kullanımı

Kilitli tile'ı iki farklı amaçla kullanabilirsin:

- Puzzle engeli: Oyuncu, kilitli tile üzerindeki itemı ancak merge planı yaparak kullanır.
- Base expansion alanı: Oyuncu, yeterli Facility Power ile tile'a tıklayıp açar.

Kilitli tile üstüne hedef zincirin kritik tek kopyasını koyarken dikkatli ol. Oyuncu o itemı merge edemeyecek konuma gelirse soft-lock oluşabilir.

## 5. Level Hedefi, Ödül ve Level Akışı

### Çalışma Kuralı

Level sahnesinde `LevelManager` şu alanlarla çalışır:

- `TargetItem`: merge sonucu bu item üretilirse level tamamlanır.
- `RewardItem`: level tamamlanınca inventory'ye eklenir.
- `ChronoChargeReward`: level sonunda verilecek Chrono Charge miktarı.
- `WinPanel` veya `LevelCompleteUI`: level tamamlanma UI'ı.
- `BaseSceneName`: devam butonu ile dönülecek base sahnesi. Varsayılan `BaseScene`.

`GridManager.OnMergeCompleted` event'i ile üretilen item takip edilir. Üretilen item `TargetItem` referansı ile aynıysa level biter.

### Editör Kurulumu

Yeni level sahnesi için minimum kurulum:

1. Sahnede `GridManager` olmalı.
2. Sahnede kayıtlı `GridTileView` tile'ları olmalı.
3. Başlangıç item prefabları doğru tile dünya pozisyonlarına yerleştirilmeli.
4. Her item prefabında `MergeableObject.ItemData` doğru atanmalı.
5. Sahnede `LevelManager` olmalı.
6. `TargetItem`, `RewardItem`, gerekirse `ChronoChargeReward` atanmalı.
7. Level sahnesi Build Settings'e eklenmeli.

### Base'e Ödül Dönüşü

Level tamamlanınca `RewardItem.ItemID` inventory'ye yazılır. BaseScene açılınca `BaseManager` inventory'deki itemları sırayla base griddeki ilk boş ve kilitsiz tile'a yerleştirir.

İlk boş tile arama sırası:

- Önce `y = 0` satırı, sonra `x = 0`dan sağa doğru.
- Sonra bir sonraki `y` satırı.

Base grid doluysa item inventory'de kalır ve uyarı loglanır.

## 6. Chrono Charge

### Çalışma Kuralı

Chrono Charge level giriş maliyeti ve level ödülü olarak kullanılır.

`LevelSelector` level açarken:

- `requireChronoCharge` açıksa giriş maliyeti kontrol edilir.
- Yeterli Chrono Charge yoksa level yüklenmez.
- Yeterliyse maliyet düşülür, save alınır ve sahne yüklenir.

`LevelManager` level bitince:

- `ChronoChargeReward > 0` ise oyuncuya ekler.

### Editör Kurulumu

BaseScene'deki `LevelSelector` üzerinde:

- `requireChronoCharge`: level girişinde kaynak gerekip gerekmediği.
- `defaultChronoCost`: özel tanım yoksa varsayılan maliyet.
- `levelChronoCosts`: level sahne adına özel maliyet listesi.
  - `LevelSceneName`: Build Settings'teki sahne adıyla birebir aynı olmalı.
  - `ChronoCost`: o levelın giriş maliyeti.

UI için:

- `ChronoChargeUI.chronoChargeText` atanır.
- `prefix` istenirse "CC: " gibi kullanılabilir.

## 7. Time Credit ve Anomaly Market

### Time Credit Çalışma Kuralı

Time Credit save içinde tutulur. Şu an ana kazanım yolu base'de obje satmaktır. Market alışlarında harcanır.

### Market Satın Alma

`MarketManager.marketItems` listesine item eklenir. UI sadece şu itemları listeler:

- Item null değil.
- `Prefab` atanmış.
- `BuyPrice > 0`.

Satın alma sırasında:

- Oyuncunun Time Credit'i `BuyPrice` kadar olmalı.
- Base gridde boş ve kilitsiz tile bulunmalı.
- Item ilk boş uygun tile'a spawn olur.
- Save alınır.

### Market Satış

Satış iki şekilde desteklenir:

- Obje sürüklenirken `sellZoneRect` içine bırakılırsa satılır.
- Kodda seçili obje satışı için `SelectObjectForSelling` ve `TrySellSelectedItem` akışı da var; UI bağlanırsa kullanılabilir.

Bir obje şu durumlarda satılamaz:

- `SellPrice <= 0`.
- Obje `IsInactiveAnomalyItem` ise.
- Objenin bulunduğu tile kilitliyse.
- Objenin `ItemData` yoksa.

### Editör Kurulumu

BaseScene'de `MarketManager`:

- `marketItems`: satışa konacak item dataları.
- `itemListContainer`: market item UI'larının ekleneceği parent.
- `marketItemUIPrefab`: üzerinde `MarketItemUI` bulunan UI prefab.
- `feedbackText`: hata/başarı mesajı.
- `marketPanel`: aç/kapatılacak market paneli.
- `sellZoneRect`: sürükle-bırak satış alanı.
- `sellZoneVisual`: obje sürüklenirken gösterilecek görsel.

UI için:

- `TimeCreditUI.timeCreditText` atanır.
- Market item prefabında `itemNameText`, `priceText`, `buyButton` bağlanır.
- Mevcut item data'da icon alanı olmadığı için `MarketItemUI.iconImage` şu an kapatılıyor.

### Dengeleme Notu

Market, soft-lock çözümü olarak kullanılabilir. Ancak düşük level itemları çok ucuz olursa level/base progression bypass edilebilir. Özellikle hedef zincirinin erken halkalarını markete koyarken level zorluğunu tekrar test et.

## 8. Facility Power ve Base Genişletme

### Çalışma Kuralı

Facility Power sadece `BaseScene` içinde çalışır. `FacilityPowerManager` başka sahnede kendini devre dışı bırakır.

Hesap:

- `CurrentFacilityPower`: base gridde duran tüm itemların `FacilityPoint` toplamı.
- `UsedFacilityPower`: açılmış expansion tile'larının tükettiği FP toplamı.
- `AvailableFacilityPower`: `CurrentFacilityPower - UsedFacilityPower`.

Tile açma:

- Oyuncu kilitli tile'a tıklar.
- Tile bir `FacilityExpansionGroup` içinde olmalı.
- `AvailableFacilityPower >= RequiredPower` ise tile açılır.
- Açılan tile save'e yazılır.
- Aynı gruptaki her tile, ayrı ayrı `RequiredPower` kadar FP tüketir.

Örnek:

- Grup `RequiredPower = 10`, içinde 3 tile var.
- Oyuncu 1 tile açarsa `UsedFacilityPower = 10`.
- 3 tile da açılırsa `UsedFacilityPower = 30`.

### Editör Kurulumu

BaseScene'de `FacilityPowerManager`:

- `expansionGroups`: açılabilir tile grupları.
  - `GroupId`: tasarım notu/isim. Örnek: `Base_Ring_01`.
  - `RequiredPower`: bu gruptaki tek bir tile'ı açma maliyeti.
  - `TilesToUnlock`: açılabilecek `GridTileView` referansları.
- `feedbackText`: eksik FP/başarı mesajı.
- `playErrorSoundOnFailedUnlock`: FP yetmeyince hata sesi.
- `feedbackVisibleSeconds`: mesaj süresi.
- `unlockSuccessMessage`: başarı mesajı.
- `unlockVFXPrefab`, `unlockVFXLifetime`: açılma efekti.

Expansion tile'larında:

- `GridTileView.StartLocked` açık olmalı.
- `UnlockableMaterial` atanmalı.
- Tile mutlaka ilgili `expansionGroups.TilesToUnlock` listesine eklenmeli.

Önemli: `UsedFacilityPower` hesabı, açılmış tile'ın başlangıçta `StartLocked` olmasına bakıyor. Expansion olarak kullanılacak tile'larda `StartLocked` kapalı kalırsa FP tüketimi doğru hesaplanmaz.

### Level/Base Design Etkisi

- FP veren itemlar base progression'ın ana iticisidir.
- Level ödülleri yüksek FP verirse oyuncu daha hızlı alan açar.
- Aynı FP maliyetine sahip birden fazla tile, oyuncuya açma sırası özgürlüğü verir.
- Dar base başlangıcı + düşük FP, market ve satış kullanımını artırır.
- Çok yüksek `RequiredPower`, oyuncuyu aynı itemları uzun süre saklamaya zorlar ve grid tıkanıklığını artırır.

## 9. Anomaly Type 1 - Random Rift

### Çalışma Kuralı

Her başarılı merge sonrası `AnomalyManager` Type 1'i denemeye alır.

Koşullar:

- `enableType1` açık olmalı.
- Random roll `anomalyChance` değerinin altında olmalı.
- Aday tile kayıtlı olmalı, kilitli olmamalı ve üzerinde anomaly olmamalı.

Seçilen tile:

- `GridManager.LockTile` ile kilitlenir.
- `riftEffectPrefab` ve `persistentRiftEffectPrefab` spawn edilir.
- Tile unlock olunca Type 1 efektleri temizlenir.

Type 1 boş tile'ı da, üzerinde obje olan açık tile'ı da kilitleyebilir. Üzerinde obje varsa o obje sürüklenemez; ancak merge grubuna dahil edilerek kullanılabilir ve tile açılabilir.

### Editör Kurulumu

Level sahnesinde `AnomalyManager`:

- `enableType1`: Type 1 aktif/pasif.
- `anomalyChance`: her merge sonrası çıkma ihtimali, 0-100.
- `riftEffectPrefab`: anlık/ana rift efekti.
- `persistentRiftEffectPrefab`: kilit sürdükçe kalan efekt.

### Tasarım Kullanımı

- Düşük şans: hafif kaos ve replay variation.
- Orta şans: oyuncunun planlarını bozarak puzzle baskısı.
- Yüksek şans: kısa gridlerde soft-lock riskini ciddi artırır.

Öneri: Çok dar veya hedef zinciri az kopyalı levellarda Type 1'i düşük tut veya kapat.

## 10. Anomaly Type 2 - Orb Rift

### Genel Çalışma Kuralı

Type 2, sahnede önceden belirlenmiş orb tile'ları üzerinden çalışır.

Level başında:

- `initialAnomalyOrbTiles` listesindeki tile'lar orb olarak kaydedilir.
- Bu tile'ların `hasAnomaly` durumu açılır.
- `anomalyOrbEffectPrefab` varsa orb görseli spawn edilir.

Her merge sonrası:

- `enableType2` açıksa Type 2 denemesi yapılır.
- `allowMultipleType2ActivationsPerMerge` açıksa her orb ayrı ayrı roll atar.
- Kapalıysa mevcut orblardan biri seçilir ve sadece o orb roll atar.
- Roll `type2ActivationChance` altındaysa orb aktive olur.

Aktifleşen orb, ağırlıklarla bir etki seçer. Etki uygulanamazsa en fazla 4 deneme yapılır.

### 3x3 Etki Alanı

Type 2 etkilerinin çoğu orb merkezli 3x3 alanda çalışır:

- Merkez tile dahil edilir.
- Grid sınırı dışındaki pozisyonlar yok sayılır.
- Çapraz tile'lar bu 3x3 alana dahildir.

### Effect Weights

Inspector alanları:

- `lockOpenTileWeight`
- `swapLockedItemWeight`
- `spawnForeignItemWeight`
- `teleportOrbWeight`

Bu değerler yüzde değil ağırlıktır. Toplam ağırlık içindeki paylarına göre seçilir.

Örnek:

- Lock = 40
- Swap = 25
- Spawn = 20
- Teleport = 15
- Toplam = 100 olduğu için yaklaşık yüzde gibi okunabilir.

Toplam 200 de olabilir; oranlar aynı mantıkla çalışır. Tüm ağırlıklar 0 olursa default olarak `LockOpenTile` seçilir.

### Type 2 Etkileri

#### 1. LockOpenTile

Orb çevresindeki 3x3 alanda:

- Kayıtlı tile olmalı.
- Kilitli olmamalı.
- Üzerinde anomaly olmamalı.

Bu adaylardan biri seçilir ve kilitlenir. Type 1 rift görselleri de kullanılır.

Not: Bu efekt boş tile'ı da, obje olan açık tile'ı da kilitleyebilir.

#### 2. SwapLockedItem

Orb çevresindeki 3x3 alanda:

- Kilitli ve üzerinde obje olan tile'lar kaynak olarak toplanır.
- Açık, boş ve anomaly olmayan tile'lar hedef olarak toplanır.

Bir kilitli tile içindeki obje, seçilen açık boş tile'a taşınır. Kaynak tile kilitli kalır ama içi boşalır.

Tasarım etkisi:

- Kilitli tile içindeki kritik itemı dışarı çıkararak oyuncuya fırsat yaratabilir.
- Aynı zamanda planlanmış kilit puzzle'ını bozabilir.

#### 3. SpawnForeignItem

Orb çevresindeki 3x3 alanda:

- Açık, boş ve anomaly olmayan tile aranır.
- `foreignEraItems` listesinden prefabı olan rastgele item seçilir.
- Seçilen item spawn edilir.
- Spawn edilen obje `IsInactiveAnomalyItem = true` olur.

Inactive anomaly item:

- Oyuncu tarafından sürüklenemez.
- Satılamaz.
- Üzerine aynı item sürüklenirse merge grubuna dahil olma ihtimali vardır; bu yüzden foreign pool itemlarını hedef zincirleriyle çakıştırırken dikkatli kullan.

#### 4. TeleportOrb

Orb başka tile'a taşınır.

`keepOrbsWithinConfiguredTiles` açıksa:

- Orb sadece `initialAnomalyOrbTiles` listesindeki tile'lar arasında dolaşır.
- Üzerinde başka anomaly olmayan tile'a gider.

Kapalıysa:

- Sahnedeki kayıtlı ve anomaly olmayan herhangi bir tile hedef olabilir.

Tasarım önerisi: Orb hareketinin kontrollü kalması için `keepOrbsWithinConfiguredTiles` açık bırak ve sadece güvenli anchor tile'ları listeye koy.

### Editör Kurulumu

Level sahnesinde `AnomalyManager`:

- `enableType2`: Type 2 aktif/pasif.
- `type2ActivationChance`: her merge sonrası aktivasyon ihtimali.
- `allowMultipleType2ActivationsPerMerge`: bir merge'de birden fazla orb aktive olabilir mi.
- `initialAnomalyOrbTiles`: level başında orb olacak tile referansları.
- `keepOrbsWithinConfiguredTiles`: teleport sadece configured tile setinde mi kalacak.
- `anomalyOrbEffectPrefab`: orb görseli.
- Effect ağırlıkları.
- `foreignEraItems`: SpawnForeignItem için item havuzu.
- `verboseType2Logs`: debug log.
- `forcedType2Effect`: test için belirli etkiyi zorla çalıştırma. Yayın/normal tasarımda `None` kalmalı.

### Orb Yerleşim Önerileri

- Orb'u hedef item zincirinin tam üstüne değil, etkileşim alanı tasarlanmış bir bölgeye koy.
- 3x3 alan içinde en az bir açık tile yoksa `LockOpenTile` ve `SpawnForeignItem` boşa düşer.
- 3x3 alan içinde kilitli ve üzerinde obje olan tile yoksa `SwapLockedItem` boşa düşer.
- Teleport için configured tile sayısı 1 ise teleport çoğunlukla uygulanamaz.
- Dar levelda birden fazla orb + çoklu aktivasyon, merge başına çok fazla durum değiştirir.

## 11. Quest Sistemi

### Çalışma Kuralı

Quest verileri `Create > Merge Game > Level Quest` ile oluşturulur.

`LevelQuestData` alanları:

- `Description`: UI açıklaması.
- `Icon`: görev ikon görseli.
- `questType`: görev tipi.
- `TargetItem`: takip edilecek item.
- `RequiredAmount`: gereken üretim sayısı.

Mevcut çalışan davranış:

- `ProduceItem`: `GridManager.OnMergeCompleted` ile üretilen item takip edilir.
- `TargetItem` üretilirse progress 1 artar.
- Progress `RequiredAmount` değerine ulaşınca UI checkmark gösterir.

Mevcut sınırlama:

- `MergeCountLimit` enum'da var ama davranışı uygulanmamış.
- Quest tamamlanması level completion'ı engellemiyor veya ödül vermiyor.
- Quest progress save'e yazılmıyor.

### Editör Kurulumu

Level sahnesinde `LevelQuestManager`:

- `LevelQuests`: sahnede gösterilecek quest assetleri.
- `QuestContainer`: quest UI parent.
- `QuestUIPrefab`: üzerinde `QuestUI_Item` bulunan prefab.

`QuestUI_Item` prefabında:

- `IconImage`
- `DescriptionPanel`
- `DescriptionText`
- `ProgressText`
- `Checkmark`
- İsteğe bağlı `QuestTabSound`, `audioSource`

### Tasarım Kullanımı

Şu an questleri zorunlu hedef yerine yönlendirme/ek challenge gibi düşün:

- "2 adet L2 üret"
- "1 adet hedef öncesi item üret"
- "Yan zinciri dene"

Levelın gerçek bitişi hâlâ `LevelManager.TargetItem` üretimine bağlıdır.

## 12. Save/Load Tasarım Notları

Save sistemi şu verileri tutar:

- BaseScene'deki itemlar ve grid pozisyonları.
- Kilitli tile listesi.
- Başlangıçta kilitli olup oyuncu tarafından açılmış tile listesi.
- Inventory item ID listesi.
- Time Credit.
- Chrono Charge.

Level sahnesindeyken save sadece inventory/economy verisini günceller; base objeleri korunur.

Şu an save'e dahil olmayanlar:

- Level içi anlık durum.
- Anomaly state.
- Quest progress.
- Completed level / unlock progression listesi.

Tasarım etkisi:

- Level restart/scene reload durumları kalıcı level state beklememeli.
- Base expansion save edilir.
- Base item yerleşimi save edilir.
- Anomaly ile değişen level durumu kalıcı değildir.

## 13. Yeni Level Kurulum Checklist

1. Sahne Build Settings'e eklendi mi?
2. `GridManager` var mı?
3. Tüm tile'larda `GridTileView.GridPosition` doğru mu?
4. Tile dünya pozisyonları grid koordinatlarıyla uyumlu mu?
5. Başlangıç item prefablarında `MergeableObject.ItemData` doğru mu?
6. Tüm kullanılan item dataları benzersiz `ItemID` taşıyor mu?
7. Merge zincirindeki her itemın `NextLevelItem` alanı doğru mu?
8. `LevelManager.TargetItem` doğru item assetine referans veriyor mu?
9. `RewardItem` base'de spawn edilebilir prefablı bir item mı?
10. Level hedef itemına ulaşmak için en az 3'lü bağlı merge rotası mümkün mü?
11. Kilitli tile varsa oyuncunun açma/merge rotası var mı?
12. Anomaly açıksa soft-lock riski test edildi mi?
13. Quest varsa `LevelQuestManager` ve UI prefab referansları bağlı mı?
14. Level giriş maliyeti gerekiyorsa BaseScene `LevelSelector.levelChronoCosts` listesine eklendi mi?

## 14. Base / Market / Facility Checklist

1. BaseScene'de `SaveManager`, `BaseManager`, `GridManager` var mı?
2. `SaveManager.AllGameItems` kullanılan itemları kapsıyor mu?
3. Market itemlarının `BuyPrice > 0` ve `Prefab` alanı dolu mu?
4. Satılacak itemların `SellPrice > 0` mı?
5. FP progression itemlarında `FacilityPoint` değerleri anlamlı mı?
6. Expansion tile'larında `StartLocked` açık mı?
7. Expansion tile'ları `FacilityPowerManager.expansionGroups` içine ekli mi?
8. Grup `RequiredPower` değeri tile başına maliyet olarak dengelendi mi?
9. `UnlockableMaterial` atanmış mı?
10. Time Credit, Chrono Charge ve Facility Power UI text referansları bağlı mı?

## 15. Dengeleme ve Soft-Lock Riskleri

Yüksek riskli durumlar:

- Hedef zincirinin tek kritik itemını kilitli/ulaşılamaz yapmak.
- Çok dar grid + yüksek Type 1 şansı.
- Birden fazla Type 2 orb + `allowMultipleType2ActivationsPerMerge` açık + yüksek activation chance.
- `foreignEraItems` havuzuna hedef zinciriyle aynı itemları koymak.
- Marketi hedef zincirini bypass edecek kadar ucuz yapmak.
- Base expansion için çok yüksek FP maliyeti koyup oyuncuyu gridde item saklamaya zorlamak.
- Levelda yeterli boş tile bırakmamak.

Pratik test kuralı:

- Level hedefi en kötü anomaly varyasyonlarında da ulaşılabilir olmalı.
- Oyuncu yanlış hamle yapsa bile satma/market/base expansion gibi sistemler soft-lock çıkışı verebilmeli.
- Her level en az bir "temiz çözüm rotası" ve bir "hatalı ama toparlanabilir rota" ile test edilmeli.

## 16. Hızlı Ayar Reçeteleri

### Hafif Anomaly Level

- `enableType1 = true`
- `anomalyChance = 10-15`
- `enableType2 = false` veya 1 orb
- Geniş grid ve bol boş tile

### Kontrollü Orb Puzzle

- `enableType1 = false`
- `enableType2 = true`
- `type2ActivationChance = 20-30`
- `allowMultipleType2ActivationsPerMerge = false`
- `keepOrbsWithinConfiguredTiles = true`
- 2-3 configured orb anchor tile

### Kaotik Challenge Level

- `enableType1 = true`
- `anomalyChance = 25+`
- `enableType2 = true`
- Birden fazla orb
- Foreign spawn pool dolu
- Mutlaka ekstra boş tile ve soft-lock testleri yapılmalı

### Base Expansion Odaklı Progression

- Level ödüllerine artan `FacilityPoint` ver.
- İlk expansion gruplarını düşük `RequiredPower` ile başlat.
- Sonraki halkalarda tile başına maliyeti artır.
- Market alış fiyatlarını, FP veren itemların değerini boşa düşürmeyecek şekilde tut.

## 17. Mevcut Implementasyon Sınırları

Bu rehber mevcut kod davranışını anlatır. Aşağıdaki sistemler tasarımda var veya kısmen var ama tam ürün davranışı değildir:

- Completed level/unlock progression data-driven değil.
- Questler level completion'a şart olarak bağlanmıyor.
- `MergeCountLimit` quest tipi çalışmıyor.
- Anomaly state save/load edilmiyor.
- Foreign anomaly item yaşam döngüsü sınırlı; sürüklenemez/satılamaz ama aynı item merge grubuna dahil olabilir.
- Market icon desteği item data'da yok.
- Level içi save/load yok.

Bu sınırları level tasarımında "garanti çalışan sistem" gibi kullanma; gerekiyorsa önce kod tarafında netleştirilmesi gerekir.
