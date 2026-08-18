// ==========================================================
// GLOBAL CHART.JS CONFIGURATION
// ==========================================================

Chart.defaults.color = '#94a3b8';
Chart.defaults.font.family = "'Segoe UI', system-ui, sans-serif";


// ==========================================================
// 1. K/D PROGRESSION CHART
// Glowing Cyan Area Line Chart
// ==========================================================

const kdCanvas = document.getElementById('kdChart');

if (kdCanvas) {

    const kdCtx = kdCanvas.getContext('2d');

    // Create cyan gradient for the area underneath the line
    const cyanGradient = kdCtx.createLinearGradient(0, 0, 0, 250);

    cyanGradient.addColorStop(
        0,
        'rgba(6, 182, 212, 0.35)'
    );

    cyanGradient.addColorStop(
        1,
        'rgba(6, 182, 212, 0.0)'
    );


    new Chart(kdCtx, {

        type: 'line',

        data: {

            labels: [
                'Sync #1',
                'Sync #2',
                'Sync #3',
                'Sync #4',
                'Sync #5',
                'Sync #6'
            ],

            datasets: [

                {
                    label: 'K/D Ratio',

                    data: [
                        1.8,
                        2.4,
                        2.9,
                        3.4,
                        3.8,
                        4.2
                    ],

                    borderColor: '#06b6d4',

                    borderWidth: 3,

                    backgroundColor: cyanGradient,

                    fill: true,

                    tension: 0.4,

                    pointBackgroundColor: '#06b6d4',

                    pointBorderColor: '#06b6d4',

                    pointRadius: 4,

                    pointHoverRadius: 6
                }

            ]

        },


        options: {

            responsive: true,

            maintainAspectRatio: true,


            plugins: {

                legend: {
                    display: false
                }

            },


            scales: {

                y: {

                    beginAtZero: true,

                    grid: {
                        color: 'rgba(255, 255, 255, 0.05)'
                    },

                    ticks: {
                        color: '#94a3b8'
                    }

                },


                x: {

                    grid: {
                        display: false
                    },

                    ticks: {
                        color: '#94a3b8'
                    }

                }

            }

        }

    });

}


// ==========================================================
// 2. WIN RATE GAME MODE CHART
// ==========================================================

const winCanvas = document.getElementById('winRateChart');

if (winCanvas) {

    const winCtx = winCanvas.getContext('2d');


    new Chart(winCtx, {

        type: 'bar',

        data: {

            labels: [
                'Solo',
                'Duos',
                'Squads',
                'Ranked'
            ],

            datasets: [

                {
                    label: 'Win Rate %',

                    data: [
                        14,
                        28,
                        45,
                        32
                    ],

                    backgroundColor: '#3b82f6',

                    borderRadius: 4

                }

            ]

        },


        options: {

            responsive: true,

            maintainAspectRatio: true,


            plugins: {

                legend: {
                    display: false
                }

            },


            scales: {

                y: {

                    beginAtZero: true,

                    max: 50,

                    grid: {
                        color: 'rgba(255, 255, 255, 0.05)'
                    },

                    ticks: {
                        color: '#94a3b8'
                    }

                },


                x: {

                    grid: {
                        display: false
                    },

                    ticks: {
                        color: '#94a3b8'
                    }

                }

            }

        }

    });

}


// ==========================================================
// 3. PRO COMBAT RADAR GRAPH
// ==========================================================

const radarCanvas = document.getElementById('radarChart');

if (radarCanvas) {

    const radarCtx = radarCanvas.getContext('2d');


    new Chart(radarCtx, {

        type: 'radar',

        data: {

            labels: [
                'Combat',
                'Accuracy',
                'Building',
                'Survival',
                'Rotation',
                'Utility'
            ],

            datasets: [

                {
                    label: 'Current Skill Matrix',

                    data: [
                        88,
                        72,
                        91,
                        65,
                        84,
                        78
                    ],

                    backgroundColor: 'rgba(236, 72, 153, 0.25)',

                    borderColor: '#ec4899',

                    borderWidth: 2,

                    pointBackgroundColor: '#ec4899',

                    pointBorderColor: '#ec4899',

                    pointRadius: 3,

                    pointHoverRadius: 5
                }

            ]

        },


        options: {

            responsive: true,

            maintainAspectRatio: true,


            plugins: {

                legend: {
                    display: false
                }

            },


            scales: {

                r: {

                    min: 0,

                    max: 100,


                    angleLines: {
                        color: 'rgba(255, 255, 255, 0.1)'
                    },


                    grid: {
                        color: 'rgba(255, 255, 255, 0.08)'
                    },


                    pointLabels: {

                        color: '#94a3b8',

                        font: {
                            size: 11,
                            family: 'Rajdhani'
                        }

                    },


                    ticks: {

                        display: false,

                        maxTicksLimit: 4

                    }

                }

            }

        }

    });

}


// ==========================================================
// 4. SYNC TELEMETRY BUTTON
// ==========================================================

function triggerSync() {

    const icon = document.getElementById('syncIcon');

    const timestamp = document.getElementById('syncTimestamp');


    // Make sure the elements exist
    if (!icon || !timestamp) {
        return;
    }


    // Start spinning animation
    icon.classList.add('spin-animation');


    // Update timestamp text
    timestamp.innerText = 'Syncing from FortniteAPI.io...';


    // Simulate API synchronization
    setTimeout(() => {

        icon.classList.remove('spin-animation');

        timestamp.innerText = 'Just now';

    }, 1200);

}