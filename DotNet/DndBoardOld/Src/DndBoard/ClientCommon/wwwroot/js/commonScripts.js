
function getElementOffsets(el) {
    var elRect = el.getBoundingClientRect();
    var obj = {};
    obj.offsetLeft = Math.floor(elRect.left);
    obj.offsetTop = Math.floor(elRect.top);
    return JSON.stringify(obj);
}

var iconsInstancesComponentInstance;
function initIconsInstancesComponent(instance) {
    iconsInstancesComponentInstance = instance;
    window.requestAnimationFrame(redraw);
}
function redraw() {
    iconsInstancesComponentInstance.invokeMethodAsync('Redraw');
    window.requestAnimationFrame(redraw);
}
function clearMapCanvas() {
    var mapCanvas = document.getElementById('IconsInstancesCanvasDiv').getElementsByTagName('canvas')[0];
    var canvasW = mapCanvas.getBoundingClientRect().width;
    var canvasH = mapCanvas.getBoundingClientRect().height;

    var ctx = mapCanvas.getContext('2d');
    ctx.clearRect(0, 0, canvasW, canvasH);
    ctx.fillStyle = 'rgb(0,200,0)';
    ctx.fillRect(0, 0, canvasW, canvasH);
}
function redrawImage(img, x, y) {
    var mapCanvas = document.getElementById('IconsInstancesCanvasDiv').getElementsByTagName('canvas')[0];;
    var ctx = mapCanvas.getContext('2d');
    ctx.drawImage(img, x, y);
}
function redrawAllImages(imgList) {
    clearMapCanvas();
    for (var i = 0; i < imgList.length; i++) {
        redrawImage(imgList[i].ref, imgList[i].x, imgList[i].y);
    }
}

/*
var boardIdGlobal;
var componentInstanceGlobal;
function onInputChange(e) {
    const fd = new FormData();
    // add all selected files
    Array.prototype.forEach.call(e.target.files, (file) => {
        fd.append('file', file);
    });
    // create the request
    const xhr = new XMLHttpRequest();
    xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) {
            componentInstanceGlobal.invokeMethodAsync('ReloadFilesIds');
        }
    };

    fd.append('boardId', boardIdGlobal);
    // path to server would be where you'd normally post the form to
    xhr.open('POST', '/Images/PostFiles', true);
    xhr.send(fd);
}
function reinitializeFileInput(boardId, input, componentInstance) {
    boardIdGlobal = boardId;
    componentInstanceGlobal = componentInstance;
    input.removeEventListener('change', onInputChange);
    input.addEventListener('change', onInputChange);
}

        var img = document.querySelector('img')

        function loaded() {
            alert('loaded');
        }

        if (img.complete) {
            loaded();
        } else {
            img.addEventListener('load', loaded);
          img.addEventListener('error', function() {
            alert('error')
          });
        }
*/