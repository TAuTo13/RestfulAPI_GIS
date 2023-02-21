// TO MAKE THE MAP APPEAR YOU MUST
// ADD YOUR ACCESS TOKEN FROM
// https://account.mapbox.com
mapboxgl.accessToken = "pk.eyJ1Ijoidm9pZC10MiIsImEiOiJja3NjZnA5MWowZzU4Mm9udmd1dTU3OTMyIn0.Bt8dNKjhVXwQExxhV1cEmA";
const map = new mapboxgl.Map({
  container: "map",
  style: "mapbox://styles/mapbox/light-v10",
  center: [-96, 37.8],
  zoom: 2
});

let apiURL = "http://localhost:7071/api/mapPoint";//apiのUR

map.on("load", async () => {
  // apiからgeojson形式で地点の情報を全てGET
  const res = await fetch(
    apiURL
  );
  const data = await res.json();
  console.log(data);
  var jsondata =
  {
    "type": "geojson",
    "data": data
  }

  console.log(data);//debug


  map.loadImage(
    "https://docs.mapbox.com/mapbox-gl-js/assets/custom_marker.png",
    (error, image) => {
      if (error) throw error;
      map.addImage("custom-marker", image);//地点に表示するマーカーの画像を取得してmapに加える
      map.addSource("points", jsondata);//取得したjsonデータを格納

      // Add a symbol layer
      map.addLayer({
        "id": "points",
        "type": "symbol",
        "source": "points",
        "layout": {
          "icon-image": "custom-marker",
          // 地点の名称の情報をマーカーに加える
          "text-field": ["get", "name"],
          "text-font": [
            "Open Sans Semibold",
            "Arial Unicode MS Bold"
          ],
          "text-offset": [0, 1.25],
          "text-anchor": "top"
        }
      });

      //マーカーがクリックされたときの処理
      map.on("click", "points", (e) => {
        // 位置情報を取得
        const coordinates = e.features[0].geometry.coordinates.slice();
        // その他の地点情報を取得
        const name = e.features[0].properties.name;
        const address = e.features[0].properties.address;
        const phone = e.features[0].properties.phone;
        const url = e.features[0].properties.url;

        console.log(phone);//debug
        console.log(url);//debug

        //popupに表示する内容を地点情報からHTMLで成形
        var description = "<strong>"+name+"</strong>"+"<br>"+
                            "["+coordinates[0]+","+coordinates[1]+"]"+"<br>"+
                            "address:"+address+"<br>";
        if(phone){
          description+="tel:"+phone+"<br>";
        }
        if(url){
          description+="<a href=\""+url+"\" target=\"_blank\">"+"website"+"</a>";
        }

        // Ensure that if the map is zoomed out such that multiple
        // copies of the feature are visible, the popup appears
        // over the copy being pointed to.
        //　popupの位置の調整
        while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
          coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
        }

        console.log(description);//debug

        //pouupを生成してmapに加える
        new mapboxgl.Popup()
          .setLngLat(coordinates)
          .setHTML(description)
          .addTo(map);
      });

      //マウスポインターの変更処理
      // Change the cursor to a pointer when the mouse is over the places layer.
      map.on("mouseenter", "points", () => {
        map.getCanvas().style.cursor = "pointer";
      });
      // Change it back to a pointer when it leaves.
      map.on("mouseleave", "points", () => {
        map.getCanvas().style.cursor = "";
      });
    }
  );
});
