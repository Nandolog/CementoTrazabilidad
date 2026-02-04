// ========================================
// 📊 CHART HELPERS - Funciones para Chart.js
// ========================================

// ✅ LOG DE INICIO
console.log('🚀 Iniciando carga de chart-helpers.js...');
console.log('📦 Chart.js disponible:', typeof Chart !== 'undefined');

let chartDistribucion = null;
let chartParadas = null;
let chartComparacion = null;

// ========================================
// 🧹 LIMPIAR GRÁFICOS
// ========================================
window.limpiarGraficos = function() {
    console.log('🧹 Llamando a limpiarGraficos...');
    
    if (chartDistribucion) {
        chartDistribucion.destroy();
        chartDistribucion = null;
    }
    if (chartParadas) {
        chartParadas.destroy();
        chartParadas = null;
    }
    if (chartComparacion) {
        chartComparacion.destroy();
        chartComparacion = null;
    }
    console.log('✅ Gráficos limpiados');
};

// ========================================
// 📊 GRÁFICO 1: Distribución del Tiempo (Pie Chart)
// ========================================
window.renderizarGraficoDistribucion = function(labels, data, colors) {
    console.log('📊 Renderizando gráfico de distribución...', { labels, data, colors });
    
    const ctx = document.getElementById('chartDistribucionTiempo');
    if (!ctx) {
        console.error('❌ Canvas chartDistribucionTiempo no encontrado');
        return;
    }

    if (chartDistribucion) {
        chartDistribucion.destroy();
    }

    chartDistribucion = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderColor: '#ffffff',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        font: {
                            size: 12
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const horas = (value / 60).toFixed(2);
                            const minutos = Math.round(value);
                            return `${label}: ${horas}h (${minutos} min)`;
                        }
                    }
                }
            }
        }
    });

    console.log('✅ Gráfico de Distribución renderizado');
};

// ========================================
// 📊 GRÁFICO 2: Paradas Detalladas (Bar Chart Horizontal)
// ========================================
window.renderizarGraficoParadasDetalladas = function(labels, data) {
    console.log('📊 Renderizando gráfico de paradas...', { labels, data });
    
    const ctx = document.getElementById('chartParadasDetalladas');
    if (!ctx) {
        console.error('❌ Canvas chartParadasDetalladas no encontrado');
        return;
    }

    if (chartParadas) {
        chartParadas.destroy();
    }

    chartParadas = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Minutos de Parada',
                data: data,
                backgroundColor: [
                    'rgba(220, 53, 69, 0.7)',   // Mecánicas - rojo
                    'rgba(255, 193, 7, 0.7)',   // Eléctricas - amarillo
                    'rgba(23, 162, 184, 0.7)',  // Operativas - cyan
                    'rgba(108, 117, 125, 0.7)'  // Circunstanciales - gris
                ],
                borderColor: [
                    'rgba(220, 53, 69, 1)',
                    'rgba(255, 193, 7, 1)',
                    'rgba(23, 162, 184, 1)',
                    'rgba(108, 117, 125, 1)'
                ],
                borderWidth: 2
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const value = context.parsed.x || 0;
                            const horas = (value / 60).toFixed(2);
                            return `${value} min (${horas}h)`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Minutos'
                    }
                }
            }
        }
    });

    console.log('✅ Gráfico de Paradas Detalladas renderizado');
};

// ========================================
// 📊 GRÁFICO 3: Comparación Real vs Objetivo (Bar Chart Agrupado)
// ========================================
window.renderizarGraficoComparacion = function(
    tnReal, tnObjetivo,
    horasReal, horasObjetivo,
    paletsReal, paletsObjetivo
) {
    console.log('📊 Renderizando gráfico de comparación...', {
        tnReal, tnObjetivo, horasReal, horasObjetivo, paletsReal, paletsObjetivo
    });
    
    const ctx = document.getElementById('chartComparacion');
    if (!ctx) {
        console.error('❌ Canvas chartComparacion no encontrado');
        return;
    }

    if (chartComparacion) {
        chartComparacion.destroy();
    }

    chartComparacion = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Producción (Tn/h)', 'Horas Productivas', 'Palets del Turno'],
            datasets: [
                {
                    label: 'Real',
                    data: [tnReal, horasReal, paletsReal],
                    backgroundColor: 'rgba(28, 200, 138, 0.7)',
                    borderColor: 'rgba(28, 200, 138, 1)',
                    borderWidth: 2
                },
                {
                    label: 'Objetivo',
                    data: [tnObjetivo, horasObjetivo, paletsObjetivo],
                    backgroundColor: 'rgba(54, 185, 204, 0.7)',
                    borderColor: 'rgba(54, 185, 204, 1)',
                    borderWidth: 2
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            weight: 'bold'
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.dataset.label || '';
                            const value = context.parsed.y || 0;
                            const metricName = context.label;
                            
                            if (metricName === 'Producción (Tn/h)') {
                                return `${label}: ${value.toFixed(2)} Tn/h`;
                            } else if (metricName === 'Horas Productivas') {
                                return `${label}: ${value.toFixed(2)} horas`;
                            } else {
                                return `${label}: ${Math.round(value)} palets`;
                            }
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Valores'
                    }
                }
            }
        }
    });

    console.log('✅ Gráfico de Comparación renderizado');
};

// ✅ VERIFICAR CARGA FINAL
console.log('✅ chart-helpers.js cargado completamente');
console.log('📋 Funciones disponibles:', {
    limpiarGraficos: typeof window.limpiarGraficos,
    renderizarGraficoDistribucion: typeof window.renderizarGraficoDistribucion,
    renderizarGraficoParadasDetalladas: typeof window.renderizarGraficoParadasDetalladas,
    renderizarGraficoComparacion: typeof window.renderizarGraficoComparacion
});