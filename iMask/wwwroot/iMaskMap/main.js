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

var image0Icon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/fysh711426/fysh711426.github.io/master/iMaskMap/image/iMask_back_0.png',
    iconSize: [86, 86],
    iconAnchor: [43, 86],
    popupAnchor: [1, -86]
});

var image1Icon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/fysh711426/fysh711426.github.io/master/iMaskMap/image/iMask_back_1.png',
    iconSize: [86, 86],
    iconAnchor: [43, 86],
    popupAnchor: [1, -86]
});

var image2Icon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/fysh711426/fysh711426.github.io/master/iMaskMap/image/iMask_back_2.png',
    iconSize: [86, 86],
    iconAnchor: [43, 86],
    popupAnchor: [1, -86]
});

var image3Icon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/fysh711426/fysh711426.github.io/master/iMaskMap/image/iMask_back_3.png',
    iconSize: [86, 86],
    iconAnchor: [43, 86],
    popupAnchor: [1, -86]
});

var imageClass = ["image0-icon", "image1-icon", "image2-icon", "image3-icon"];

var markers = new L.MarkerClusterGroup({
    iconCreateFunction: function (cluster) {
        var list = cluster.getAllChildMarkers();
        var level = 0;
        for (var i = 0; i < list.length; i++) {
            if (level < 3 && list[i].options.icon.options.iconUrl === image3Icon.options.iconUrl)
                level = 3;
            else if (level < 2 && list[i].options.icon.options.iconUrl === image2Icon.options.iconUrl)
                level = 2;
            else if (level < 1 && list[i].options.icon.options.iconUrl === image1Icon.options.iconUrl)
                level = 1;
        }
        return L.divIcon({ html: '<div><span>' + cluster.getChildCount() + '</span></div>', className: "icon-cluster " + imageClass[level], iconSize: [50, 50] });
    },
    removeOutsideVisibleBounds: true,
    animate: true
}).addTo(map);

var xhr = new XMLHttpRequest();
xhr.open('get', 'https://raw.githubusercontent.com/kiang/pharmacies/master/json/points.json');
xhr.send();
xhr.onload = function () {
    var data = JSON.parse(xhr.responseText).features;
    for (var i = 0; i < data.length; i++) {

        var imageIcon = image0Icon;
        if (data[i].properties.mask_adult >= 50)
            imageIcon = image3Icon;
        else if (data[i].properties.mask_adult > 20)
            imageIcon = image2Icon;
        else if (data[i].properties.mask_adult > 0)
            imageIcon = image1Icon;

        var mark = L.marker([
            data[i].geometry.coordinates[1],
            data[i].geometry.coordinates[0]
        ], { icon: imageIcon }
        ).bindPopup(
            '<p class="popup-name">' + data[i].properties.name + '<p/>' +
            '<p class="popup-phone">[電話] ' + data[i].properties.phone + '<p/>' +
            '<p class="popup-mask">[口罩] 成人: ' + data[i].properties.mask_adult + '、兒童: ' + data[i].properties.mask_child + '<p/>' +
            '<p class="popup-address">[地址] ' + data[i].properties.address + '<p/>');
        markers.addLayer(mark);
    }
    map.addLayer(markers);
};