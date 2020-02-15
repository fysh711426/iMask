var map = L.map('map', {
    center: [24.2113218, 120.6942998],
    zoom: 4
});

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
}).addTo(map);

//L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
//    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
//}).addTo(map);

var xhr = new XMLHttpRequest();
xhr.open('get','https://services1.arcgis.com/0MSEUqKaxRlEPj5g/arcgis/rest/services/ncov_cases/FeatureServer/1/query?f=json&where=1%3D1&returnGeometry=false&spatialRel=esriSpatialRelIntersects&outFields=*&orderByFields=Confirmed%20desc%2CCountry_Region%20asc%2CProvince_State%20asc&resultOffset=0&resultRecordCount=250&cacheHint=true');
xhr.send();
xhr.onload = function () {

    var title = document.getElementById("popup-title");
    var confirmed = document.getElementById("popup-confirmed");
    var recovered = document.getElementById("popup-recovered");
    var deaths = document.getElementById("popup-deaths");

    var data = JSON.parse(xhr.responseText).features;

    //將地名轉成中文
    for (let i = 0; i < data.length; i++) {
        let item = data[i].attributes;
        item.Country_Region = item.Country_Region || "";
        item.Country_Region = typeof translation(item.Country_Region) === "undefined" ?
            item.Country_Region : translation(item.Country_Region);
        item.Province_State = item.Province_State || "";
        item.Province_State = typeof translation(item.Province_State) === "undefined" ?
            item.Province_State : translation(item.Province_State);
    }

    //計算全球人數
    var totalConfirmed = 0;
    var totalRecovered = 0;
    var totalDeaths = 0;
    for (let i = 0; i < data.length; i++) {
        let item = data[i].attributes;
        totalConfirmed += item.Confirmed;
        totalRecovered += item.Recovered;
        totalDeaths += item.Deaths;
    }

    var focus = null;

    //處理 mapClick
    var mapClick = function () {
        title.innerHTML = '全球';
        confirmed.innerHTML = '[確診] ' + totalConfirmed + ' 人';
        recovered.innerHTML = '[康復] ' + totalRecovered + ' 人';
        deaths.innerHTML = '[死亡] ' + totalDeaths + ' 人';

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

    //畫圓圈
    for (let i = 0; i < data.length; i++) {
        let item = data[i].attributes;

        //計算圓圈大小
        var radius = getRadius(item.Confirmed);

        var click = function (e) {
            var circle = e.target;
            title.innerHTML = circle.data.Country_Region +
                (circle.data.Province_State === '' ||
                circle.data.Province_State === circle.data.Country_Region ?
                '' : ' - ' + circle.data.Province_State);
            confirmed.innerHTML = '[確診] ' + circle.data.Confirmed + ' 人';
            recovered.innerHTML = '[康復] ' + circle.data.Recovered + ' 人';
            deaths.innerHTML = '[死亡] ' + circle.data.Deaths + ' 人';

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

        var circle = L.circleMarker([item.Lat, item.Long_], {
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

    //處理列表
    var list = _(data).map(function (item) {
        return item.attributes;
    }).groupBy(function (item) {
        return item.Country_Region;
    }).map(function (list, index) {
        return {
            Country_Region: index,
            Confirmed: _.reduce(list, function (sum, n) {
                return sum + n.Confirmed;
            }, 0),
            Recovered: _.reduce(list, function (sum, n) {
                return sum + n.Recovered;
            }, 0),
            Deaths: _.reduce(list, function (sum, n) {
                return sum + n.Deaths;
            }, 0)
        };
    }).orderBy(['Confirmed', 'Country_Region'], ['desc', 'asc']).value();

    //產生列表 HTML
    var result = "";
    for (let i = 0; i < list.length; i++) {
        let item = list[i];
        result += '<p>[' + item.Country_Region + '] ' + item.Confirmed + ' 人</p >';
    }
    document.getElementById("list").innerHTML = result;

    //顯示切換按鈕
    document.getElementById("btn").style.display = "block";
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
        "\"Diamond Princess\" cruise ship": "鑽石公主遊輪",
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
        "Others": "其他",
        "Egypt": "埃及"
    };
    return dic[text];
}

function getRadius(count) {
    var radius = 0;
    if (count >= 1000000) {
        radius = 85;
        radius = radius + 3.0 * (parseInt(count / 1000000) % 10);
    }
    else if (count >= 100000) {
        radius = 60;
        radius = radius + 2.5 * (count / 100000);
    }
    else if (count >= 10000) {
        radius = 40;
        radius = radius + 2.0 * (count / 10000);
    }
    else if (count >= 1000) {
        radius = 25;
        radius = radius + 1.5 * (count / 1000);
    }
    else if (count >= 100) {
        radius = 15;
        radius = radius + 1.0 * (count / 100);
    }
    else if (count >= 0) {
        radius = 5;
        radius = radius + 1.0 * (count / 10);
    }
    return radius;
}

var state = 1;
function toggle() {
    var item = document.getElementById("item-block");
    var list = document.getElementById("list-block");
    if (state === 1) {
        item.style.display = "none";
        list.style.display = "block";
        state = 2;
    }
    else {
        item.style.display = "block";
        list.style.display = "none";
        state = 1;
    }
}