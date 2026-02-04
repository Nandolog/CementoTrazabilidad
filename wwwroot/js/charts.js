// Funciones para renderizar gráficos con Chart.js

window.renderizarGraficoParadas = function (labels, data) {
    console.log('🎨 renderizarGraficoParadas llamada');
    console.log('Labels:', labels);
    console.log('Data:', data);
    
    const ctx = document.getElementById('chartParadas');
    
    if (!ctx) {
        console.error('❌ Canvas chartParadas no encontrado');
        return;
    }

    console.log('✅ Canvas encontrado:', ctx);

    // Destruir gráfico anterior si existe
    if (window.chartParadasInstance) {
        console.log('🗑️ Destruyendo gráfico anterior');
        window.chartParadasInstance.destroy();
    }

    // Crear nuevo gráfico de dona
    console.log('🎨 Creando nuevo gráfico...');
    window.chartParadasInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                label: 'Minutos de parada',
                data: data,
                backgroundColor: [
                    '#f6c23e', // amarillo
                    '#e74a3b', // rojo
                    '#4e73df', // azul
                    '#1cc88a', // verde
                    '#36b9cc', // cyan
                    '#858796'  // gris
                ],
                borderColor: '#fff',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed;
                            const hours = Math.floor(value / 60);
                            const minutes = Math.round(value % 60);
                            return `${label}: ${hours}h ${minutes}m`;
                        }
                    }
                }
            }
        }
    });
    
    console.log('✅ Gráfico creado exitosamente');
};

window.renderizarGraficoProduccion = function (labels, data) {
    console.log('🎨 renderizarGraficoProduccion llamada');
    console.log('Labels:', labels);
    console.log('Data:', data);
    
    const ctx = document.getElementById('chartProduccion');
    
    if (!ctx) {
        console.error('❌ Canvas chartProduccion no encontrado');
        return;
    }

    console.log('✅ Canvas encontrado:', ctx);

    // Destruir gráfico anterior si existe
    if (window.chartProduccionInstance) {
        console.log('🗑️ Destruyendo gráfico anterior');
        window.chartProduccionInstance.destroy();
    }

    // Crear nuevo gráfico de barras
    console.log('🎨 Creando nuevo gráfico...');
    window.chartProduccionInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Ciclos de carga',
                data: data,
                backgroundColor: '#4e73df',
                borderColor: '#2e59d9',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
    
    console.log('✅ Gráfico creado exitosamente');
};