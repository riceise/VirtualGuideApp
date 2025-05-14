// wwwroot/js/mapInterop.js

var mapInstance = null;
var dotNetReference = null;
var mapGlApiReady = false; // Флаг готовности API
var pendingInitOptions = null;

window.onMapGlApiLoaded = () => {
    console.log("MapGL API (mapgl) is now loaded.");
    mapGlApiReady = true;
    if (pendingInitOptions) {
        window.initMap(
            pendingInitOptions.mapContainerId,
            pendingInitOptions.apiKey,
            pendingInitOptions.centerLat,
            pendingInitOptions.centerLng,
            pendingInitOptions.zoom,
            pendingInitOptions.dotNetRef,
            true // Флаг, что API уже точно готово
        );
        pendingInitOptions = null; // Сбрасываем
    }
};
// Функция инициализации карты
window.initMap = (mapContainerId, apiKey, centerLat, centerLng, zoom, dotNetRef, apiIsReady = false) => {
    dotNetReference = dotNetRef;

    if (!mapGlApiReady && !apiIsReady) {
        // ... (логика отложенной инициализации остается) ...
        return Promise.resolve("MapGL API loading, initMap queued.");
    }

    // ОЧИСТКА КОНТЕЙНЕРА ПЕРЕД ИНИЦИАЛИЗАЦИЕЙ КАРТЫ
    const mapContainer = document.getElementById(mapContainerId);
    if (mapContainer) {
        mapContainer.innerHTML = ''; // Очищаем содержимое
    } else {
        console.error(`Map container with id '${mapContainerId}' not found.`);
        return Promise.reject(`Map container with id '${mapContainerId}' not found.`);
    }

    if (mapInstance) {
        mapInstance.destroy();
        mapInstance = null;
    }

    try {
        console.log("Attempting to initialize MapGL map in container:", mapContainerId);
        mapInstance = new mapgl.Map(mapContainerId, {
            center: [centerLng, centerLat],
            zoom: zoom,
            key: apiKey
        });

        // ... (обработчик 'click' и 'load') ...

        console.log("MapGL map initialized successfully.");
        return Promise.resolve("Map initialized successfully with MapGL API");

    } catch (error) {
        console.error("Error initializing MapGL map:", error);
        return Promise.reject("Error initializing MapGL map: " + error.message);
    }
};
// Функция добавления маркера
window.addMarkerToMap = (lat, lng, popupText) => { // popupText пока не используется, нужно смотреть API для попапов
    if (mapInstance) {
        new mapgl.Marker(mapInstance, {
            coordinates: [lng, lat], // MapGL API ожидает [долгота, широта]
            // Для popupText нужно будет смотреть, как MapGL API обрабатывает их.
            // Возможно, через label или кастомный HTML.
            // Пример с label (простой текст):
            // label: {
            //    text: popupText || '',
            //    offset: [0, -20], // смещение текста относительно маркера
            //    fontSize: '12px',
            //    color: '#000000'
            // }
        });
    } else {
        console.error("MapGL instance not available to add marker.");
    }
};

// Функция отображения маршрута (полилинии)
// pointsArray - массив массивов [[lng1, lat1], [lng2, lat2], ...]
window.drawRouteOnMap = (pointsArray, color = 'blue', width = 3) => { // Обратите внимание на 'width' вместо 'weight'
    if (mapInstance && pointsArray && pointsArray.length > 1) {
        // Для рисования линии в MapGL JS API нужно использовать источники данных (Data Source) и слои (Layer)
        // Это сложнее, чем просто DG.polyline.
        // Примерно так:
        const routeId = 'route-' + Date.now(); // Уникальный ID для источника и слоя

        // 1. Создаем GeoJSON объект для линии
        const routeGeoJson = {
            type: 'Feature',
            geometry: {
                type: 'LineString',
                coordinates: pointsArray // массив координат [[lng, lat], [lng, lat], ...]
            },
            properties: {}
        };

        // 2. Добавляем источник данных
        mapInstance.addSource(routeId, {
            type: 'geojson',
            data: routeGeoJson
        });

        // 3. Добавляем слой для отображения линии
        mapInstance.addLayer({
            id: routeId + '-layer',
            type: 'line',
            source: routeId,
            paint: {
                'line-color': color,
                'line-width': width
            }
        });

        // Подгонка карты под маршрут (требует вычисления bounds)
        // const bounds = pointsArray.reduce((bounds, coord) => {
        //     return bounds.extend(coord);
        // }, new mapgl.LngLatBounds(pointsArray[0], pointsArray[0]));
        // mapInstance.fitBounds(bounds, { padding: 20 });

    } else {
        console.error("MapGL instance or points not available to draw route.");
    }
};

// Функция центрирования карты
window.setMapView = (lat, lng, zoom) => {
    if (mapInstance) {
        mapInstance.setCenter([lng, lat]); // MapGL API ожидает [долгота, широта]
        mapInstance.setZoom(zoom);
    }
};

// Функция очистки объектов
// С MapGL это сложнее, т.к. объекты добавляются через источники и слои.
// Нужно удалять конкретные слои и источники по их ID.
window.clearMapObjects = () => {
    if (mapInstance) {
        // Это очень грубый способ, он удалит ВСЕ слои и источники,
        // включая базовую карту, если не быть осторожным.
        // Лучше запоминать ID добавленных слоев/источников и удалять их по ID.

        // Пример: если вы знаете ID добавленных слоев и источников
        // const layerIdsToRemove = ['route-xxxx-layer', 'another-layer-id'];
        // const sourceIdsToRemove = ['route-xxxx', 'another-source-id'];
        // layerIdsToRemove.forEach(id => { if (mapInstance.getLayer(id)) mapInstance.removeLayer(id); });
        // sourceIdsToRemove.forEach(id => { if (mapInstance.getSource(id)) mapInstance.removeSource(id); });

        console.warn("clearMapObjects for MapGL is more complex. Implement specific removal logic.");
    }
};

window.disposeMap = () => {
    if (mapInstance) {
        mapInstance.destroy();
        mapInstance = null;
    }
    if (dotNetReference) {
        dotNetReference.dispose();
        dotNetReference = null;
    }
    mapGlApiReady = false; // Сбрасываем флаг при уничтожении
    pendingInitOptions = null;
    console.log("MapGL map disposed.");
};