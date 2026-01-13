# eMERGEncy - Uygulama Analizi ve Plan (Sadece Kod)

Kapsam notu:
- Bu plan sadece GDD’deki kod sistemlerini ve mekanikleri kapsar.
- Asset veya art pipeline taskleri dahil edilmez.

Mevcut durum (koddan gözlenen):
- Grid yerleşimi ve merge-3 mantığı var (GridManager, MergeableObject, GridTileView).
- Kilitli grid tile görsel toggle ve sürükleme engeli ile mevcut.
- Quest sistemi “ProduceItem” türü için var ve merge complete event’e bağlı.
- Mergeable item verisi için ScriptableObject mevcut (MergeableItemData).

GDD’de olup henüz uygulanmayan mekanikler (kod boşlukları):
- Chrono Charge kaynağı (zaman yolculuğu para birimi) ve kazanma/harcama kuralları.
- Anomaly Market (fiyatlarla sat/al, erişim koşulları).
- Facility Power (base genişleme eşikleri ve grid açma).
- Kapalı Grid detayları (kilitli tile merge’e dahil olunca açılır, kısmen ele alındı).
- Anomali Zaman Yarığı davranışı (merge sonrası 3x3 etki, rastgele olasılık).
- Quest sistemi genişlemesi (diğer quest türleri, ödüller, zincirleme).
- Zaman kırılması / merge sayısı limiti ve cezalar.
- Research binası mantığı (item tüketimi, hikaye/teknoloji çıktısı).
- Main Base sistemi (kalıcı grid, level arası akış).
- Save/Load (base, level, para birimleri, ilerleme, questler, anomaliler, timerlar).

Riskler ve bağımlılıklar:
- Grid sistemi değişiklikleri çoğu mekaniği etkiler; önce core merge mantığını koru.
- Save/Load stabil veri modeline ihtiyaç duyar (item, tile, quest ID’leri).
- Rastgele anomali etkileri, save/resume gerekiyorsa deterministik veya seed’li olmalı.

Önerilen plan (aşamalar, code-first):

Aşama 1 - Veri modeli ve eventler (temel)
- Mergeable item, tile, quest ve anomaly için ID ve serialize kuralları tanımla.
- MergeableItemData’yı mevcut ekonomi alanlarıyla (BuyPrice, SellPrice) kullan,
  gerekirse market/research için EraId/Category gibi opsiyonel alanlar ekle.
- Merkezi GameState/SessionState (ScriptableObject veya düz C#) ekle:
  para birimleri, mevcut level, base grid durumu, quest ilerlemesi, anomaly durumu.
- Event’leri standardize et: OnMergeCompleted, OnItemMoved, OnItemSold, OnItemBought,
  OnTileLocked/Unlocked, OnAnomalyTriggered.

Aşama 2 - Grid/merge kurallarının GDD ile hizalanması (kilitli grid)
- Kilitli tile sadece içindeki item merge’e katıldığında açılmalı.
- Kilitli tile’a taşıma engeli devam etmeli; görsel durum veriyle senkron olmalı.
- Anomali ve quest mantığı için merge validasyon hook’ları ekle.

Aşama 3 - Quest sistemi genişlemesi (GDD Quest System)
- Quest türleri ekle (örn. MergeCountLimit, ProduceItem mevcut, muhtemelen SellItem).
- Quest ödüllerini uygula (para birimi, unlock vb.).
- Quest ilerlemesini Save/Load’a dahil et.

Aşama 4 - Ekonomi sistemleri (Chrono Charge + Anomaly Market)
- Chrono Charge’ı kazan/harca kurallarıyla uygula.
- MergeableItemData’daki SellPrice/BuyPrice ile sat/al uygula.
- Market erişimini level/koşullara göre sınırla.

Aşama 5 - Facility Power ve base genişletme
- Main Base grid’ine yerleştirilen objelerden Facility Power hesapla.
- Eşikler sağlanınca grid genişlemesini aç.
- Base grid ve genişlemeleri kalıcı kaydet.

Aşama 6 - Anomali Zaman Yarığı sistemi
- Grid tile’larında anomaly durumunu temsil et (hasAnomaly zaten var).
- Her merge sonrası 3x3 bölgede anomali tetikleme olasılığı uygula.
- Etkileri uygula:
  - Açık bir grid’i kilitle.
  - Kilitli grid’deki objeyi taşı/değiştir.
  - Farklı zamandan obje üret (tasarıma göre işlevsiz olabilir).
  - Anomaliyi başka tile’a ışınla.
- Etkileri data-driven yap (kolay ayar için).

Aşama 7 - Main Base ve ilerleme akışı
- Level bitince base sahnesini yükle, kazanılan objeleri base envanterine taşı.
- Base grid’de merge ve hikaye kapılarına ilerlemeyi aç.
- Level progression’ı Chrono Charge harcamasına bağla.

Aşama 8 - Save/Load sistemi
- Grid tile’ları, yerleştirilmiş objeler, para birimleri, quest ilerlemesi, anomaly durumu,
  ve cooldown/timer’ları serialize et.
- Sürümlemeli save formatı ve migration stratejisi ekle.
- Save/restore doğruluğu için test harness ekle.

Aşama 9 - QA hook’ları ve debug araçları (sadece kod)
- Item spawn, tile kilitle/aç, anomali tetikleme için debug UI veya keybind ekle.
- Grid durumu tutarsızlıklarını yakalamak için logging ve assertion’lar ekle.

Önerilen yakın adımlar (küçük, güvenli):
- Veri ID’lerini ve minimal GameState struct’ını tanımla.
- Quest type enum’unu genişlet ve stub handler’lar ekle (UI değişmeden).
- Save/Load şemasını kodda taslakla (tam bağlanmamış olsa da).
