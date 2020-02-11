var latitude = getParameterByName('latitude') || 24.2113218;
var longitude = getParameterByName('longitude') || 120.6942998;

var map = L.map('map', {
    center: [latitude, longitude],
    zoom: 12
});

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
}).addTo(map);

var redIcon = new L.Icon({
    iconUrl: 'https://cdn.rawgit.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
});
L.marker([latitude, longitude], { icon: redIcon }).addTo(map);

var markers = new L.MarkerClusterGroup().addTo(map);

var imageIcon = new L.Icon({
    iconUrl: 'https://ithelp.ithome.com.tw/upload/images/20200210/20106865an2gNAyvHQ.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [40, 40],
    iconAnchor: [20, 40],
    popupAnchor: [1, -40],
    shadowSize: [40, 40]
});

var xhr = new XMLHttpRequest();
xhr.open('get', 'https://raw.githubusercontent.com/kiang/pharmacies/master/json/points.json');
xhr.send();
xhr.onload = function () {
    var data = JSON.parse(xhr.responseText).features;
    for (var i = 0; i < data.length; i++) {
        var mark = L.marker([
            data[i].geometry.coordinates[1],
            data[i].geometry.coordinates[0]
        ], {
            icon: imageIcon
        }).bindPopup(data[i].properties.name);
        markers.addLayer(mark);
    }
    map.addLayer(markers);
};