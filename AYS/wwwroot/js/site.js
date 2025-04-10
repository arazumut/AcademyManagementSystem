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
    
    // Form yönlendirme sorunlarını önle
    setupForms();
});

// Form işleme ve sorunları önleme
function setupForms() {
    // Login formu
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        // Formun yanlış yönlendirilmesini önle
        loginForm.addEventListener('click', function(e) {
            // Eğer tıklanan öğe bir input, label veya form içi buton ise ve formun kendisi değilse
            if (e.target !== loginForm && (e.target.tagName === 'INPUT' || e.target.tagName === 'LABEL' || e.target.tagName === 'BUTTON')) {
                e.stopPropagation(); // Olay yayılımını durdur
            }
        });
        
        // Çift gönderimi önle
        loginForm.addEventListener('submit', function(e) {
            const submitButton = document.getElementById('login-submit');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerText = 'Giriş Yapılıyor...';
                setTimeout(function() {
                    submitButton.disabled = false;
                    submitButton.innerText = 'Giriş Yap';
                }, 3000);
            }
        });
    }
    
    // Kayıt formu
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        // Formun yanlış yönlendirilmesini önle
        registerForm.addEventListener('click', function(e) {
            // Eğer tıklanan öğe bir input, label veya form içi buton ise ve formun kendisi değilse
            if (e.target !== registerForm && (e.target.tagName === 'INPUT' || e.target.tagName === 'LABEL' || e.target.tagName === 'BUTTON' || e.target.tagName === 'SELECT')) {
                e.stopPropagation(); // Olay yayılımını durdur
            }
        });
        
        // Çift gönderimi önle
        registerForm.addEventListener('submit', function(e) {
            const submitButton = document.getElementById('registerSubmit');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerText = 'Kaydolunuyor...';
                setTimeout(function() {
                    submitButton.disabled = false;
                    submitButton.innerText = 'Kaydol';
                }, 3000);
            }
        });
    }
} 