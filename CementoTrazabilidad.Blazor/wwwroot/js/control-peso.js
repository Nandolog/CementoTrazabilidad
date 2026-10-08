// ============================================
// GRÁFICOS DE CONTROL DE PESO
// ============================================

let chartTendenciaPeso = null;
let chartBoquillas = null;

window.renderizarGraficoTendenciaPeso = function (labels, datos, objetivo) {
    const ctx = document.getElementById('chartTendenciaPeso');
    if (!ctx) return;

    if (chartTendenciaPeso) {
        chartTendenciaPeso.destroy();
    }

    chartTendenciaPeso = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Peso Promedio (kg)',
                data: datos,
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                tension: 0.3,
                pointRadius: 6,
                pointHoverRadius: 8
            }, {
                label: 'Objetivo',
                data: labels.map(() => objetivo),
                borderColor: 'rgb(255, 99, 132)',
                borderDash: [5, 5],
                pointRadius: 0,
                fill: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top' },
                title: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    ticks: { callback: (v) => v + ' kg' }
                }
            }
        }
    });
};

window.renderizarGraficoBoquillas = function (labels, datos, objetivo) {
    const ctx = document.getElementById('chartBoquillas');
    if (!ctx) return;

    if (chartBoquillas) {
        chartBoquillas.destroy();
    }

    const colores = datos.map(d => {
        const dif = Math.abs(d - objetivo);
        if (dif <= 0.1) return 'rgba(75, 192, 192, 0.7)';
        if (dif <= 0.3) return 'rgba(255, 206, 86, 0.7)';
        return 'rgba(255, 99, 132, 0.7)';
    });

    chartBoquillas = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Peso Promedio (kg)',
                data: datos,
                backgroundColor: colores,
                borderColor: colores.map(c => c.replace('0.7', '1')),
                borderWidth: 1
            }, {
                label: 'Objetivo',
                data: labels.map(() => objetivo),
                type: 'line',
                borderColor: 'rgb(255, 99, 132)',
                borderDash: [5, 5],
                pointRadius: 0,
                fill: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top' }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    ticks: { callback: (v) => v + ' kg' }
                }
            }
        }
    });
};

// Función auxiliar para descargar archivos
window.descargarArchivo = function (base64, fileName, mimeType) {
    const link = document.createElement('a');
    link.href = `data:${mimeType};base64,${base64}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};