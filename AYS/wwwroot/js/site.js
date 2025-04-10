// Genel sayfa fonksiyonları
document.addEventListener('DOMContentLoaded', function() {
    console.log('Akademi Yönetim Sistemi başarıyla yüklendi!');
    
    // Bootstrap tooltip'lerini etkinleştir
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Kartlara tıklama olayı - Form içeren kartlar hariç
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        // Kart içinde form olup olmadığını kontrol et
        const hasForm = card.querySelector('form') !== null;
        
        // Form içermeyen kartlara tıklama işlevselliği ekle
        if (!hasForm) {
            card.addEventListener('click', function(e) {
                // Kart içindeki bağlantılar, butonlar hariç kart tıklamasını ele al
                if (!e.target.closest('a') && !e.target.closest('button') && !e.target.closest('form')) {
                    const link = this.querySelector('a');
                    if (link) {
                        link.click();
                    }
                }
            });
        }
    });
    
    // Form yönlendirme sorunlarını önle
    setupForms();
});

// Form işleme ve sorunları önleme
function setupForms() {
    // Login formu
    setupSpecificForm('loginForm', 'login-submit', 'Giriş Yapılıyor...', 'Giriş Yap');
    
    // Kayıt formu
    setupSpecificForm('registerForm', 'registerSubmit', 'Kaydolunuyor...', 'Kaydol');
    
    // Sınav ekleme formu
    setupSpecificForm('examCreateForm', 'examSubmitButton', 'Kaydediliyor...', 'Kaydet');
    
    // Kurs ekleme formu
    setupSpecificForm('courseCreateForm', 'courseSubmitButton', 'Kaydediliyor...', 'Kaydet');
    
    // Sınav sonucu ekleme formu
    setupSpecificForm('examResultCreateForm', 'examResultSubmitButton', 'Kaydediliyor...', 'Kaydet');
    
    // Diğer formlar için genel form işleyicisi
    setupAllOtherForms();
}

// Belirli bir formu ayarla
function setupSpecificForm(formId, buttonId, loadingText, defaultText) {
    const form = document.getElementById(formId);
    if (form) {
        // Formun yanlış yönlendirilmesini önle
        form.addEventListener('click', function(e) {
            // Form içindeki tüm form elemanlarını durdur
            const isFormElement = e.target !== form && (
                e.target.tagName === 'INPUT' || 
                e.target.tagName === 'TEXTAREA' || 
                e.target.tagName === 'SELECT' || 
                e.target.tagName === 'LABEL' || 
                e.target.tagName === 'BUTTON' ||
                e.target.closest('div.form-check') !== null
            );
            
            if (isFormElement) {
                e.stopPropagation(); // Olay yayılımını durdur
            }
        });
        
        // Çift gönderimi önle
        form.addEventListener('submit', function(e) {
            const submitButton = document.getElementById(buttonId);
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = submitButton.innerHTML.replace(defaultText, loadingText);
                setTimeout(function() {
                    submitButton.disabled = false;
                    submitButton.innerHTML = submitButton.innerHTML.replace(loadingText, defaultText);
                }, 3000);
            }
        });
    }
}

// Diğer tüm formları ayarla (ID'si olmayan formlar için)
function setupAllOtherForms() {
    // Sayfadaki ID'si olmayan tüm formları bul
    const allForms = document.querySelectorAll('form:not([id])');
    
    allForms.forEach(form => {
        // Her form için olay dinleyicisi ekle
        form.addEventListener('click', function(e) {
            // Form içindeki tüm form elemanlarını durdur
            if (e.target !== form && 
                (e.target.tagName === 'INPUT' || 
                 e.target.tagName === 'TEXTAREA' || 
                 e.target.tagName === 'SELECT' || 
                 e.target.tagName === 'LABEL' || 
                 e.target.tagName === 'BUTTON')) {
                e.stopPropagation(); // Olay yayılımını durdur
            }
        });
        
        // Bu formun submit düğmesini bul
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            // Form gönderiminde düğmeyi devre dışı bırak
            form.addEventListener('submit', function() {
                submitButton.disabled = true;
                
                // 3 saniye sonra tekrar etkinleştir
                setTimeout(function() {
                    submitButton.disabled = false;
                }, 3000);
            });
        }
    });
} 