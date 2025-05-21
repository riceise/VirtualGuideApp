let mapInstance = null;
let dotNetReference = null;
let drawnLayers = [];

window.initLeafletMap = (mapContainerId, centerLat, centerLng, zoom, dotNetRef) => {
    dotNetReference = dotNetRef;
    console.log(`Attempting to initialize Leaflet map for container: ${mapContainerId}`);

    const mapContainer = document.getElementById(mapContainerId);
    if (!mapContainer) {
        console.error(`Map container '${mapContainerId}' not found. Retrying...`);
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                const retryContainer = document.getElementById(mapContainerId);
                if (retryContainer) {
                    console.log(`Retry successful: Map container '${mapContainerId}' found.`);
                    initializeMap(retryContainer, mapContainerId, centerLat, centerLng, zoom);
                    resolve("Leaflet map initialized successfully.");
                } else {
                    console.error(`Map container '${mapContainerId}' still not found after retry.`);
                    reject("Map container not found.");
                }
            }, 100);
        });
    }

    console.log(`Clearing map container content for '${mapContainerId}'.`);
    mapContainer.innerHTML = '';
    return initializeMap(mapContainer, mapContainerId, centerLat, centerLng, zoom);
};

function initializeMap(mapContainer, mapContainerId, centerLat, centerLng, zoom) {
    if (mapInstance) {
        try {
            mapInstance.remove();
        } catch (error) {
            console.warn("Error removing existing map instance:", error);
        }
        mapInstance = null;
    }
    drawnLayers = [];

    try {
        mapInstance = L.map(mapContainerId).setView([centerLat, centerLng], zoom);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(mapInstance);

        mapInstance.on('click', function (e) {
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('OnMapClicked', e.latlng.lat, e.latlng.lng).catch(err => {
                    console.error("Error invoking OnMapClicked:", err);
                });
            }
        });

        if (dotNetReference) {
            dotNetReference.invokeMethodAsync('NotifyMapReadyInternal').catch(err => {
                console.error("Error invoking NotifyMapReadyInternal:", err);
            });
        }

        return Promise.resolve("Leaflet map initialized successfully.");
    } catch (error) {
        console.error("Error initializing Leaflet map:", error);
        return Promise.reject("Error initializing Leaflet map.");
    }
}

window.addLeafletMarker = (lat, lng, popupText) => {
    if (!mapInstance) {
        console.warn("Cannot add marker: mapInstance is null.");
        return;
    }
    try {
        const marker = L.marker([lat, lng]).addTo(mapInstance);
        if (popupText) {
            marker.bindPopup(popupText);
        }
        drawnLayers.push(marker);
    } catch (error) {
        console.error("Error adding Leaflet marker:", error);
    }
};

window.drawLeafletRoute = (routeCoordinates, color = 'blue', weight = 3) => {
    if (!mapInstance || !routeCoordinates || routeCoordinates.length < 2) {
        console.warn("Cannot draw route: invalid map instance or route coordinates.");
        return;
    }

    try {
        const latLngPoints = routeCoordinates.map(coord => [coord[1], coord[0]]);
        const polyline = L.polyline(latLngPoints, { color: color, weight: weight }).addTo(mapInstance);
        drawnLayers.push(polyline);
        mapInstance.fitBounds(polyline.getBounds());
    } catch (error) {
        console.error("Error drawing Leaflet route:", error);
    }
};

window.clearLeafletMapObjects = () => {
    if (mapInstance) {
        try {
            drawnLayers.forEach(layer => {
                mapInstance.removeLayer(layer);
            });
            drawnLayers = [];
        } catch (error) {
            console.error("Error clearing Leaflet map objects:", error);
        }
    }
};

window.fitMapToPoints = (points) => {
    if (!mapInstance || !points || points.length === 0) {
        console.warn("Cannot fit map to points: invalid map instance or points.");
        return;
    }
    try {
        const bounds = L.latLngBounds(points);
        mapInstance.fitBounds(bounds, { padding: [30, 30] });
    } catch (error) {
        console.error("Error fitting map to points:", error);
    }
};

window.disposeLeafletMap = () => {
    try {
        if (mapInstance) {
            mapInstance.remove();
            mapInstance = null;
        }
        drawnLayers = [];
        if (dotNetReference) {
            dotNetReference.dispose();
            dotNetReference = null;
        }
        console.log("Leaflet map disposed successfully.");
    } catch (error) {
        console.warn("Error disposing Leaflet map:", error);
    }
};