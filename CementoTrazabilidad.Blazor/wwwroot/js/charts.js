// charts.js - Control de gráficos del dashboard
(function () {
    console.log('🚀 Iniciando carga de charts.js...');
    console.log('📦 Chart.js disponible:', typeof Chart !== 'undefined');

    // Variables globales para los gráficos EXISTENTES
    window.chartParadas = null;
    window.chartProduccion = null;
    
    // Variables globales para los gráficos NUEVOS del Dashboard
    let chartDistribucion = null;
    let chartParadasDetalladas = null;
    let chartComparacion = null;

    // ========================================
    // 📊 GRÁFICOS EXISTENTES (mantener)
    // ========================================

    // Renderizar gráfico de paradas
    window.renderizarGraficoParadas = function (labels, data) {
        try {
            console.log('🎨 renderizarGraficoParadas iniciando');

            const canvas = document.getElementById('chartParadas');
            if (!canvas) {
                console.error('❌ Canvas chartParadas no encontrado');
                return;
            }

            // Destruir gráfico anterior si existe
            if (window.chartParadas) {
                console.log('🗑️ Destruyendo gráfico de paradas anterior');
                window.chartParadas.destroy();
                window.chartParadas = null;
            }

            const ctx = canvas.getContext('2d');

            // Si no hay datos significativos, mostrar gráfico vacío
            if (!data || data.length === 0 || (data.length === 1 && data[0] === 1.0 && labels[0] === 'Sin paradas')) {
                console.log('📊 Mostrando gráfico vacío para paradas');

                window.chartParadas = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: ['Sin paradas'],
                        datasets: [{
                            data: [1],
                            backgroundColor: ['#e9ecef'],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        plugins: {
                            legend: { display: true },
                            title: {
                                display: true,
                                text: 'No hay paradas registradas'
                            }
                        }
                    }
                });
                return;
            }

            // Crear nuevo gráfico con datos reales
            window.chartParadas = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: data,
                        backgroundColor: [
                            '#dc3545', '#ffc107', '#17a2b8',
                            '#28a745', '#6f42c1', '#fd7e14'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                padding: 20,
                                usePointStyle: true
                            }
                        },
                        title: {
                            display: true,
                            text: 'Distribución de Paradas (minutos)'
                        }
                    }
                }
            });

            console.log('✅ Gráfico de paradas creado exitosamente');
        } catch (error) {
            console.error('❌ Error en renderizarGraficoParadas:', error);
        }
    };

    // Renderizar gráfico de producción
    window.renderizarGraficoProduccion = function (labels, data) {
        try {
            console.log('🎨 renderizarGraficoProduccion iniciando');

            const canvas = document.getElementById('chartProduccion');
            if (!canvas) {
                console.error('❌ Canvas chartProduccion no encontrado');
                return;
            }

            // Destruir gráfico anterior si existe
            if (window.chartProduccion) {
                console.log('🗑️ Destruyendo gráfico de producción anterior');
                window.chartProduccion.destroy();
                window.chartProduccion = null;
            }

            const ctx = canvas.getContext('2d');

            window.chartProduccion = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Ciclos de Carga',
                        data: data,
                        backgroundColor: '#4e73df',
                        borderColor: '#2e59d9',
                        borderWidth: 1,
                        borderRadius: 4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    scales: {
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: 'Cantidad'
                            },
                            ticks: {
                                stepSize: 1
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Turnos'
                            }
                        }
                    },
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top'
                        },
                        title: {
                            display: true,
                            text: 'Ciclos de Carga por Turno'
                        }
                    }
                }
            });

            console.log('✅ Gráfico de producción creado exitosamente');
        } catch (error) {
            console.error('❌ Error en renderizarGraficoProduccion:', error);
        }
    };

    // ========================================
    // 🆕 NUEVAS FUNCIONES PARA DASHBOARD
    // ========================================

    // 🧹 Función para limpiar TODOS los gráficos (actualizada)
    window.limpiarGraficos = function () {
        try {
            console.log('🧹 Limpiando todos los gráficos');

            // Gráficos existentes
            if (window.chartParadas) {
                window.chartParadas.destroy();
                window.chartParadas = null;
            }

            if (window.chartProduccion) {
                window.chartProduccion.destroy();
                window.chartProduccion = null;
            }

            // Gráficos nuevos del Dashboard
            if (chartDistribucion) {
                chartDistribucion.destroy();
                chartDistribucion = null;
            }
            if (chartParadasDetalladas) {
                chartParadasDetalladas.destroy();
                chartParadasDetalladas = null;
            }
            if (chartComparacion) {
                chartComparacion.destroy();
                chartComparacion = null;
            }

            console.log('✅ Gráficos limpiados');
        } catch (error) {
            console.error('❌ Error limpiando gráficos:', error);
        }
    };

    // 📊 GRÁFICO 1: Distribución del Tiempo (Pie Chart)
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

    // 📊 GRÁFICO 2: Paradas Detalladas (Bar Chart Horizontal)
    window.renderizarGraficoParadasDetalladas = function(labels, data) {
        console.log('📊 Renderizando gráfico de paradas detalladas...', { labels, data });
        
        const ctx = document.getElementById('chartParadasDetalladas');
        if (!ctx) {
            console.error('❌ Canvas chartParadasDetalladas no encontrado');
            return;
        }

        if (chartParadasDetalladas) {
            chartParadasDetalladas.destroy();
        }

        chartParadasDetalladas = new Chart(ctx, {
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

    // 📊 GRÁFICO 3: Comparación Real vs Objetivo (Bar Chart Agrupado)
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

    // ✅ LOG FINAL DE CARGA
    console.log('✅ charts.js cargado completamente');
    console.log('📋 Funciones disponibles:', {
        renderizarGraficoParadas: typeof window.renderizarGraficoParadas,
        renderizarGraficoProduccion: typeof window.renderizarGraficoProduccion,
        renderizarGraficoDistribucion: typeof window.renderizarGraficoDistribucion,
        renderizarGraficoParadasDetalladas: typeof window.renderizarGraficoParadasDetalladas,
        renderizarGraficoComparacion: typeof window.renderizarGraficoComparacion,
        limpiarGraficos: typeof window.limpiarGraficos
    });
})();