// Chart.js functionality for statistics charts
let pageViewsChartInstance = null;

window.initializePageViewsChart = function (canvasId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error('Canvas element not found:', canvasId);
        return;
    }

    // Destroy existing chart if it exists
    if (pageViewsChartInstance) {
        pageViewsChartInstance.destroy();
    }

    pageViewsChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Page Views',
                data: [],
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                tension: 0.1,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
};

window.updatePageViewsChart = function (canvasId, chartData) {
    if (!pageViewsChartInstance) {
        console.error('Chart not initialized');
        return;
    }

    pageViewsChartInstance.data.labels = chartData.labels;
    pageViewsChartInstance.data.datasets[0].data = chartData.data;
    pageViewsChartInstance.update();
};
