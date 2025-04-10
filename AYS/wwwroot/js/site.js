// Genel sayfa fonksiyonları
document.addEventListener('DOMContentLoaded', function() {
    console.log('Akademi Yönetim Sistemi başarıyla yüklendi!');
    
    // Bootstrap tooltip'lerini etkinleştir
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Kartlara tıklama olayı
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('click', function(e) {
            // Kart içindeki bağlantılar hariç kart tıklamasını ele al
            if (!e.target.closest('a') && !e.target.closest('button')) {
                const link = this.querySelector('a');
                if (link) {
                    link.click();
                }
            }
        });
    });
}); 