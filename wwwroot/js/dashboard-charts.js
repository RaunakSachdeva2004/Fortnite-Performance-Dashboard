// ==========================================================
// Dashboard trend chart — driven entirely by real data passed
// in from the server (see the inline script in
// Views/Dashboard/Index.cshtml). No fabricated/sample values
// live in this file; if there's no sync history yet, the
// chart simply renders empty.
// ==========================================================

Chart.defaults.color = '#94a3b8';
Chart.defaults.font.family = "'Segoe UI', system-ui, sans-serif";

function initDashboardCharts(data) {
    const canvas = document.getElementById('trendChart');
    if (!canvas || typeof Chart === 'undefined') {
        return;
    }

    const ctx = canvas.getContext('2d');

    const cyanGradient = ctx.createLinearGradient(0, 0, 0, 250);
    cyanGradient.addColorStop(0, 'rgba(6, 182, 212, 0.35)');
    cyanGradient.addColorStop(1, 'rgba(6, 182, 212, 0.0)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [
                {
                    label: 'K/D Ratio',
                    data: data.kd,
                    borderColor: '#06b6d4',
                    backgroundColor: cyanGradient,
                    fill: true,
                    tension: 0.35,
                    yAxisID: 'yKd',
                    pointRadius: 3,
                    pointBackgroundColor: '#06b6d4'
                },
                {
                    label: 'Win Rate %',
                    data: data.winRate,
                    borderColor: '#ec4899',
                    backgroundColor: 'rgba(236, 72, 153, 0.15)',
                    fill: false,
                    tension: 0.35,
                    yAxisID: 'yWinRate',
                    pointRadius: 3,
                    pointBackgroundColor: '#ec4899'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { labels: { color: '#cbd5e1' } }
            },
            scales: {
                x: { grid: { color: 'rgba(255,255,255,0.05)' } },
                yKd: {
                    position: 'left',
                    grid: { color: 'rgba(255,255,255,0.05)' },
                    title: { display: true, text: 'K/D Ratio', color: '#06b6d4' }
                },
                yWinRate: {
                    position: 'right',
                    grid: { drawOnChartArea: false },
                    title: { display: true, text: 'Win Rate %', color: '#ec4899' }
                }
            }
        }
    });
}
