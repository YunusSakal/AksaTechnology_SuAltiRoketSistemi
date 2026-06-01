// wwwroot/js/site.js — Global helpers

// Auto-dismiss alerts after 5s
document.querySelectorAll('.alert').forEach(el => {
    setTimeout(() => {
        el.style.transition = 'opacity 0.5s';
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 500);
    }, 5000);
});

// Close modal on backdrop click
document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
    backdrop.addEventListener('click', (e) => {
        if (e.target === backdrop) backdrop.hidden = true;
    });
});

// Chart.js global defaults for dark theme
if (typeof Chart !== 'undefined') {
    Chart.defaults.color = '#9aa0b7';
    Chart.defaults.borderColor = '#1a2347';
    Chart.defaults.font.family = "'Rajdhani', system-ui, sans-serif";
}
