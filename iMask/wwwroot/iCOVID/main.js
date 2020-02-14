var map = L.map('map', {
    center: [24.2113218, 120.6942998],
    zoom: 4
});

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
}).addTo(map);

var xhr = new XMLHttpRequest();
xhr.open('get', 'https://coronavirus-tracker-api.herokuapp.com/all');
xhr.send();
xhr.onload = function () {

    var title = document.getElementById("popup-title");
    var confirmed = document.getElementById("popup-confirmed");
    var recovered = document.getElementById("popup-recovered");
    var deaths = document.getElementById("popup-deaths");

    var data = JSON.parse(xhr.responseText);

    var focus = null;

    //處理 mapClick
    var latest = data.latest;
    var mapClick = function () {
        title.innerHTML = '全球';
        confirmed.innerHTML = '[確診] ' + latest.confirmed + ' 人';
        recovered.innerHTML = '[康復] ' + latest.recovered + ' 人';
        deaths.innerHTML = '[死亡] ' + latest.deaths + ' 人';

        if (focus !== null) {
            focus.setStyle({
                fillColor: '#e91e3a',
                fillOpacity: 0.8
            });
            focus = null;
        }
    };
    map.on('click', mapClick);
    mapClick();

    //處理 JSON 資料
    var locations = getLocations(data);

    for (var index in locations) {

        let item = locations[index];

        //處理圓形大小
        var radius = getRadius(parseInt(item.confirmed.latest));

        var click = function (e) {
            var circle = e.target;
            title.innerHTML = circle.data.country +
                (circle.data.province === '' ||
                    circle.data.province === circle.data.country ?
                    '' : ' - ' + circle.data.province);
            confirmed.innerHTML = '[確診] ' + circle.data.confirmed.latest + ' 人';
            recovered.innerHTML = '[康復] ' + circle.data.recovered.latest + ' 人';
            deaths.innerHTML = '[死亡] ' + circle.data.deaths.latest + ' 人';

            //focus
            if (focus !== null) {
                focus.setStyle({
                    fillColor: '#e91e3a',
                    fillOpacity: 0.8
                });
            }
            focus = circle.setStyle({
                fillColor: '#107879',
                fillOpacity: 0.8
            });
        };

        var circle = L.circleMarker([item.lat, item.long], {
            radius: radius,
            stroke: false,
            fillColor: '#e91e3a',
            fillOpacity: 0.8,
            bubblingMouseEvents: false
        });

        //將資料放在 circle 內
        circle.data = item;

        circle.addTo(map)
            .on('click', click);
    }
};

function translation(text) {
    var dic = {
        "Anhui": "安徽省",
        "Beijing": "北京",
        "Chongqing": "重慶市",
        "Fujian": "福建",
        "Gansu": "甘肅",
        "Guangdong": "粵",
        "Guangxi": "廣西",
        "Guizhou": "貴州省",
        "Hainan": "海南",
        "Hebei": "河北省",
        "Heilongjiang": "黑龍江省",
        "Henan": "河南",
        "Hubei": "湖北",
        "Hunan": "湖南",
        "Inner Mongolia": "內蒙古",
        "Jiangsu": "江蘇省",
        "Jiangxi": "江西省",
        "Jilin": "吉林省",
        "Liaoning": "遼寧省",
        "Ningxia": "寧夏",
        "Qinghai": "青海",
        "Shaanxi": "陝西省",
        "Shandong": "山東省",
        "Shanghai": "上海",
        "Shanxi": "山西省",
        "Sichuan": "四川省",
        "Tianjin": "天津",
        "Tibet": "西藏",
        "Xinjiang": "新疆",
        "Yunnan": "雲南",
        "Zhejiang": "浙江省",
        "Taiwan": "台灣",
        "Seattle, WA": "華盛頓州西雅圖市",
        "Chicago, IL": "伊利諾伊州芝加哥",
        "Tempe, AZ": "亞利桑那坦佩",
        "Macau": "澳門",
        "Hong Kong": "香港",
        "Toronto, ON": "多倫多",
        "British Columbia": "不列顛哥倫比亞省",
        "Orange, CA": "加利福尼亞奧蘭治",
        "Los Angeles, CA": "加利福尼亞洛杉磯",
        "New South Wales": "新南威爾士州",
        "Victoria": "維多利亞州",
        "Queensland": "昆士蘭州",
        "London, ON": "倫敦",
        "Santa Clara, CA": "加利福尼亞聖克拉拉",
        "South Australia": "南澳大利亞",
        "Boston, MA": "馬薩諸塞州波士頓",
        "San Benito, CA": "加利福尼亞聖貝尼托",
        "Madison, WI": "威斯康星州麥迪遜",
        "Diamond Princess cruise ship": "鑽石公主遊輪",
        "San Diego County, CA": "加利福尼亞聖地亞哥縣",
        "San Antonio, TX": "德克薩斯州聖安東尼奧市",
        "Mainland China": "中國大陸",
        "Thailand": "泰國",
        "Japan": "日本",
        "South Korea": "南韓",
        "US": "美国",
        "Singapore": "新加坡",
        "Vietnam": "越南",
        "France": "法國",
        "Nepal": "尼泊爾",
        "Malaysia": "馬來西亞",
        "Canada": "加拿大",
        "Australia": "澳大利亞",
        "Cambodia": "柬埔寨",
        "Sri Lanka": "斯里蘭卡",
        "Germany": "德國",
        "Finland": "芬蘭",
        "United Arab Emirates": "阿拉伯聯合酋長國",
        "Philippines": "菲律賓",
        "India": "印度",
        "Italy": "意大利",
        "UK": "英國",
        "Russia": "俄國",
        "Sweden": "瑞典",
        "Spain": "西班牙",
        "Belgium": "比利時",
        "Others": "其他"
    };
    return dic[text];
}

function getLocations(data) {
    var locations = {};

    //處理 confirmed
    for (let i = 0; i < data.confirmed.locations.length; i++) {
        let item = data.confirmed.locations[i];
        locations['[' + item.country + '][' + item.province + ']'] = {
            country: typeof translation(item.country) === "undefined" ?
                item.country : translation(item.country),
            province: typeof translation(item.province) === "undefined" ?
                item.province : translation(item.province),
            lat: item.coordinates.lat,
            long: item.coordinates.long,
            confirmed: {
                latest: item.latest
            },
            recovered: {
                latest: '未知'
            },
            deaths: {
                latest: '未知'
            }
        };
    }

    //處理 recovered
    for (let i = 0; i < data.recovered.locations.length; i++) {
        let item = data.recovered.locations[i];
        let location = locations['[' + item.country + '][' + item.province + ']'];
        location.recovered.latest = item.latest;
    }

    //處理 deaths
    for (let i = 0; i < data.deaths.locations.length; i++) {
        let item = data.deaths.locations[i];
        let location = locations['[' + item.country + '][' + item.province + ']'];
        location.deaths.latest = item.latest;
    }

    return locations;
}

function getRadius(latest) {
    var radius = 0;
    if (latest >= 1000000) {
        radius = 110;
    }
    else if (latest >= 100000) {
        radius = 80;
    }
    else if (latest >= 10000) {
        radius = 55;
    }
    else if (latest >= 1000) {
        radius = 35;
    }
    else if (latest >= 100) {
        radius = 20;
    }
    else if (latest >= 10) {
        radius = 10;
    }
    else if (latest >= 0) {
        radius = 5;
    }
    return radius;
}