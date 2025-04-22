function showToast(type, message) {
    // Kiểm tra xem Toastify đã được load chưa
    if (typeof Toastify === 'undefined') {
        console.error('Toastify is not loaded');
        return;
    }

    const backgroundColor = {
        success: '#28a745',
        error: '#dc3545',
        warning: '#ffc107',
        info: '#17a2b8'
    };

    Toastify({
        text: message,
        duration: 3000,
        gravity: "top",
        position: 'right',
        backgroundColor: backgroundColor[type],
        stopOnFocus: true,
        close: true
    }).showToast();
} 