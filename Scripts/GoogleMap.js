var map;
var geocoder;
function initialize() {
    geocoder = new google.maps.Geocoder();
    var mapOptions = {
        zoom: 16
    };
    map = new google.maps.Map(document.getElementById('map-canvas'),
        mapOptions);

    // Try HTML5 geolocation
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var pos = new google.maps.LatLng(position.coords.latitude,
                                             position.coords.longitude);
            var image = '../Images/bluePoint.png';
            var marker = new google.maps.Marker({
                position: pos,
                map: map,
                icon: image
            });

            map.setCenter(pos);
            getPlace(position.coords.latitude, position.coords.longitude);
        }, function () {
            handleNoGeolocation(true);
        });
    } else {
        // Browser doesn't support Geolocation
        handleNoGeolocation(false);
    }
}

function handleNoGeolocation(errorFlag) {
    if (errorFlag) {
        var content = 'Error: The Geolocation service failed.';
    } else {
        var content = 'Error: Your browser doesn\'t support geolocation.';
    }

    var options = {
        map: map,
        position: new google.maps.LatLng(60, 105),
        content: content
    };

    var infowindow = new google.maps.InfoWindow(options);
    map.setCenter(options.position);
}

function getPlace(latitude, longitude) {
    var para = "{v_Latitude: '" + latitude + "', v_Longitude: '" + longitude + "'}";
    var url = location.pathname;
    url = url.substr(url.lastIndexOf("/") + 1);
    if (url.lastIndexOf(".aspx") < 0) { url += ".aspx"; }
    $.ajax({
        type: "POST",
        url: url + "/GetPlace",
        data: para,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: updatePlaceList,
        failure: function (error) {
            alert(error.d)
        }
    });
}

var isRun = false;
function updatePlaceList(response) {
    if (isRun == false) {
        var jsonStr = JSON.parse(response.d);
        var jsonData = jsonStr.data;
        var ul = $('<table></table>').attr({ id: 'result_tb' });
        for (var i = 0; i < jsonData.length; i++) {
            var url = jsonData[i].website;
            var pic = jsonData[i].pic;
            if (url == "") { url = null; }
            var li_id = "Li_" + i;
            var li = $('<tr></tr>').attr({ id: li_id, class: "tr_data" }).append(
                $('<td></td>').attr({ class: "td_content" }).append(
                    $('<div></div>').append(
                        $('<a></a>').attr({ id: 'lab_NAME', href: url, class: 'list-title' }).text(jsonData[i].name)
                    )).append(
                    $('<div></div>').append(
                        $('<span></span>').attr({ id: 'lab_CONTENT' }).text(jsonData[i].address)
                    )).append(
                    $('<div></div>').append(
                        $('<img />').attr({ alt: "Facebook", src: "../Images/FB-f-Logo__blue_29.png" })).append(
                        $('<span></span>').attr({ id: 'lab_CHECKINS' }).text(" " + jsonData[i].checkins))
                    )
                ).append(
                $('<td></td>').attr({ class: "td_pic" }).append(
                    $('<img />').attr({ alt: "Logo Image", src: pic })
                ));
            ul.append(li);
        }
        $('#results').append(ul);

        for (var i = 0; i < jsonData.length; i++) {
            var data = jsonData[i];
            var li_id = "Li_" + i;
            var liData = document.getElementById(li_id);
            setPlaceMark(i, data, liData);
        }
        isRun = true;
    }
}

function setPlaceMark(i, data, liData) {
    var address = data.location.street;
    var marker;
    if (address != "") {
        geocoder.geocode({ 'address': address }, function (result, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                var name = data.name;
                var infowindow = new google.maps.InfoWindow({
                    content: name
                });
                marker = new google.maps.Marker({
                    map: map,
                    position: result[0].geometry.location,
                    zIndex: i
                });
                setMarkEvent(liData, marker, infowindow);
            } else {
                alert('Geocode was not successful for the following reason: ' + status);
            }
        });
    } else {
        var pos = new google.maps.LatLng(data.location.latitude, data.location.longitude);
        var name = data.name;
        var infowindow = new google.maps.InfoWindow({
            content: name
        });
        marker = new google.maps.Marker({
            map: map,
            position: pos,
            zIndex: i
        });
        setMarkEvent(liData, marker, infowindow);
    }
}

function setMarkEvent(liData, marker, infowindow) {
    google.maps.event.addDomListener(liData, 'mouseover', function () {
        infowindow.open(marker.get('map'), marker);
    });
    google.maps.event.addDomListener(liData, 'mouseout', function () {
        infowindow.close(marker.get('map'), marker);
    });
    google.maps.event.addDomListener(marker, 'mouseover', function () {
        infowindow.open(marker.get('map'), marker);
    });
    google.maps.event.addDomListener(marker, 'mouseout', function () {
        infowindow.close(marker.get('map'), marker);
    });
}

google.maps.event.addDomListener(window, 'load', initialize);

// Search Click
function SearchPlace() {
    alert("test");
}