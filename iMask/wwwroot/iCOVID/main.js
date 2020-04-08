var map = L.map('map', {
    center: [24.2113218, 120.6942998],
    zoom: 4
});

//L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
//    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
//}).addTo(map);

L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>'
}).addTo(map);

var xhr = new XMLHttpRequest();
xhr.open('get', 'https://services1.arcgis.com/0MSEUqKaxRlEPj5g/arcgis/rest/services/ncov_cases/FeatureServer/1/query?f=json&where=1%3D1&returnGeometry=false&spatialRel=esriSpatialRelIntersects&outFields=*&orderByFields=Confirmed%20desc%2CCountry_Region%20asc%2CProvince_State%20asc&resultOffset=0&resultRecordCount=250&cacheHint=true');
xhr.send();
xhr.onload = function () {

    var title = document.getElementById("popup-title");
    var confirmed = document.getElementById("popup-confirmed");
    var recovered = document.getElementById("popup-recovered");
    var deaths = document.getElementById("popup-deaths");

    var data = JSON.parse(xhr.responseText).features;

    //將地名轉成中文
    var check = "";
    for (let i = 0; i < data.length; i++) {
        let item = data[i].attributes;
        item.Country_Region = item.Country_Region || "";
        item.Country_Region = typeof translation(item.Country_Region) === "undefined" ?
            item.Country_Region : translation(item.Country_Region);
        item.Province_State = item.Province_State || "";
        item.Province_State = typeof translation(item.Province_State) === "undefined" ?
            item.Province_State : translation(item.Province_State);
        check += item.Country_Region + "\n" + item.Province_State + "\n";
    }
    console.log(check);

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

        if (item.Lat === null || item.Long_ === null)
            continue;

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

        //console.log(item.Lat + "," + item.Long_);
        //console.log(item);
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
        "US": "美國",
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
        "Egypt": "埃及",
        "China": "中國",
        "Iran": "伊朗",
        "Korea, South": "南韓",
        "Switzerland": "瑞士",
        "United Kingdom": "英國",
        "Norway": "挪威",
        "Netherlands": "荷蘭",
        "Denmark": "丹麥",
        "Cruise Ship": "遊輪",
        "Austria": "奧地利",
        "Qatar": "卡塔爾",
        "Greece": "希臘",
        "Bahrain": "巴林",
        "Israel": "以色列",
        "Czechia": "捷克語",
        "Slovenia": "斯洛文尼亞",
        "Portugal": "葡萄牙",
        "Iceland": "冰島",
        "Brazil": "巴西",
        "Ireland": "愛爾蘭",
        "Romania": "羅馬尼亞",
        "Estonia": "愛沙尼亞",
        "Iraq": "伊拉克",
        "Kuwait": "科威特",
        "Poland": "波蘭",
        "Saudi Arabia": "沙特阿拉伯",
        "Indonesia": "印尼",
        "Lebanon": "黎巴嫩",
        "San Marino": "聖馬力諾",
        "Chile": "智利",
        "Taiwan*": "台灣*",
        "Luxembourg": "盧森堡",
        "Serbia": "塞爾維亞",
        "Slovakia": "斯洛伐克",
        "Bulgaria": "保加利亞",
        "Brunei": "文萊",
        "Albania": "阿爾巴尼亞",
        "Croatia": "克羅地亞",
        "Peru": "秘魯",
        "South Africa": "南非",
        "Algeria": "阿爾及利亞",
        "Panama": "巴拿馬",
        "Argentina": "阿根廷",
        "Pakistan": "巴基斯坦",
        "Georgia": "喬治亞州",
        "Hungary": "匈牙利",
        "Ecuador": "厄瓜多爾",
        "Belarus": "白俄羅斯",
        "Costa Rica": "哥斯達黎加",
        "Cyprus": "塞浦路斯",
        "Latvia": "拉脫維亞",
        "Mexico": "墨西哥",
        "Colombia": "哥倫比亞",
        "Oman": "阿曼",
        "Armenia": "亞美尼亞",
        "Bosnia and Herzegovina": "波斯尼亞和黑塞哥維那",
        "Malta": "馬耳他",
        "Tunisia": "突尼斯",
        "Morocco": "摩洛哥",
        "Azerbaijan": "阿塞拜疆",
        "North Macedonia": "北馬其頓",
        "Moldova": "摩爾多瓦",
        "Afghanistan": "阿富汗",
        "Dominican Republic": "多米尼加共和國",
        "Bolivia": "玻利維亞",
        "Maldives": "馬爾代夫",
        "Senegal": "塞內加爾",
        "Martinique": "馬提尼克島",
        "Jamaica": "牙買加",
        "Lithuania": "立陶宛",
        "Kazakhstan": "哈薩克斯坦",
        "New Zealand": "新西蘭",
        "Paraguay": "巴拉圭",
        "Reunion": "同學聚會",
        "French Guiana": "法屬圭亞那",
        "Turkey": "土耳其",
        "Cuba": "古巴",
        "Liechtenstein": "列支敦士登",
        "Uruguay": "烏拉圭",
        "Bangladesh": "孟加拉國",
        "Ghana": "加納",
        "Ukraine": "烏克蘭",
        "Aruba": "阿魯巴",
        "Burkina Faso": "布基納法索",
        "Cameroon": "喀麥隆",
        "Congo (Kinshasa)": "剛果(金沙薩)",
        "Honduras": "洪都拉斯",
        "Jersey": "澤西島",
        "Monaco": "摩納哥",
        "Namibia": "納米比亞",
        "Nigeria": "尼日利亞",
        "Seychelles": "塞舌爾",
        "Trinidad and Tobago": "特立尼達和多巴哥",
        "Venezuela": "委內瑞拉",
        "Andorra": "安道爾",
        "Antigua and Barbuda": "安提瓜和巴布達",
        "Bhutan": "不丹",
        "Cayman Islands": "開曼群島",
        "Cote d'Ivoire": "科特迪瓦",
        "Curacao": "庫拉索島",
        "Eswatini": "史瓦帝尼王國",
        "Ethiopia": "埃塞俄比亞",
        "Gabon": "加蓬",
        "Guadeloupe": "瓜德羅普島",
        "Guatemala": "危地馬拉",
        "Guernsey": "根西島",
        "Guinea": "幾內亞",
        "Guyana": "圭亞那",
        "Holy See": "教廷",
        "Jordan": "約旦",
        "Kenya": "肯尼亞",
        "Mauritania": "毛里塔尼亞",
        "Mongolia": "蒙古",
        "Rwanda": "盧旺達",
        "Saint Lucia": "聖盧西亞",
        "Saint Vincent and the Grenadines": "聖文森特和格林納丁斯",
        "Sudan": "蘇丹",
        "Suriname": "蘇里南",
        "Togo": "多哥",
        "Diamond Princess": "鑽石公主",
        "Washington": "華盛頓州",
        "New York": "紐約",
        "California": "加利福尼亞州",
        "Massachusetts": "馬薩諸塞州",
        "Ontario": "安大略省",
        "Colorado": "科羅拉多州",
        "Louisiana": "路易斯安那州",
        "Florida": "佛羅里達",
        "New Jersey": "新澤西州",
        "Illinois": "伊利諾伊州",
        "Texas": "德州",
        "Pennsylvania": "賓夕法尼亞州",
        "Virginia": "維吉尼亞州",
        "Alberta": "艾伯塔省",
        "Oregon": "俄勒岡州",
        "Tennessee": "田納西州",
        "Wisconsin": "威斯康星州",
        "Maryland": "馬里蘭州",
        "Ohio": "俄亥俄",
        "Michigan": "密西根州",
        "Quebec": "魁北克",
        "North Carolina": "北卡羅來納",
        "Connecticut": "康乃狄克州",
        "Grand Princess": "大公主",
        "Minnesota": "明尼蘇達州",
        "Nevada": "內華達州",
        "Rhode Island": "羅德島",
        "South Carolina": "南卡羅來納",
        "Western Australia": "澳大利亞西部",
        "Iowa": "愛荷華州",
        "Indiana": "印第安那州",
        "Kentucky": "肯塔基州",
        "Nebraska": "內布拉斯加",
        "Arizona": "亞利桑那",
        "Arkansas": "阿肯色州",
        "District of Columbia": "哥倫比亞特區",
        "New Mexico": "新墨西哥",
        "Utah": "猶他州",
        "Faroe Islands": "法羅群島",
        "South Dakota": "南達科他州",
        "Kansas": "堪薩斯州",
        "New Hampshire": "新罕布什爾",
        "Alabama": "阿拉巴馬州",
        "Delaware": "特拉華州",
        "Mississippi": "密西西比州",
        "Tasmania": "塔斯馬尼亞",
        "Fench Guiana": "圭亞那",
        "Montana": "蒙大拿",
        "Vermont": "佛蒙特",
        "Manitoba": "曼尼托巴省",
        "Hawaii": "夏威夷",
        "Missouri": "密蘇里州",
        "Oklahoma": "俄克拉荷馬州",
        "French Polynesia": "法屬波利尼西亞",
        "Maine": "緬因州",
        "Puerto Rico": "波多黎各",
        "New Brunswick": "新不倫瑞克省",
        "Saskatchewan": "薩斯喀徹溫省",
        "St Martin": "聖馬丁",
        "Channel Islands": "海峽群島",
        "Idaho": "愛達荷州",
        "Wyoming": "懷俄明州",
        "Australian Capital Territory": "澳大利亞首都領地",
        "Northern Territory": "北方領土",
        "Saint Barthelemy": "聖巴托洛繆島",
        "Gibraltar": "直布羅陀",
        "North Dakota": "北達科他州",
        "Virgin Islands, U.S.": "美國維爾京群島",
        "Uzbekistan": "烏茲別克斯坦",
        "West Virginia": "西弗吉尼亞",
        "Nova Scotia": "新斯科舍省",
        "Niger": "尼日爾",
        "Kyrgyzstan": "吉爾吉斯斯坦",
        "Mauritius": "毛里求斯",
        "West Bank and Gaza": "約旦河西岸和加沙",
        "Montenegro": "黑山共和國",
        "Newfoundland and Labrador": "紐芬蘭與拉布拉多",
        "Alaska": "阿拉斯加州",
        "Kosovo": "科索沃",
        "Mayotte": "馬約特島",
        "Isle of Man": "馬恩島",
        "Guam": "關島",
        "El Salvador": "薩爾瓦多",
        "Djibouti": "吉布地",
        "Madagascar": "馬達加斯加",
        "Barbados": "巴巴多斯",
        "Mali": "馬里",
        "Uganda": "烏干達",
        "Congo (Brazzaville)": "剛果(布拉柴維爾)",
        "Virgin Islands": "維爾京群島",
        "Sint Maarten": "聖馬丁",
        "Bermuda": "百慕大",
        "Zambia": "贊比亞",
        "Bahamas": "巴哈馬",
        "Guinea-Bissau": "幾內亞比紹",
        "Eritrea": "厄立特里亞"
    };
    return dic[text];
}