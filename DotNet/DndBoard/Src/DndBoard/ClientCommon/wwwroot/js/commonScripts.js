
function getElementOffsets(el) {
    var elRect = el.getBoundingClientRect();
    var obj = {};
    obj.offsetLeft = Math.floor(elRect.left);
    obj.offsetTop = Math.floor(elRect.top);
    return JSON.stringify(obj);
}


function createFileURLUnmarshalled(fileContent) {
    var bytes = Blazor.platform.toUint8Array(fileContent);
    var blob = new Blob([bytes], { type: 'image/png' });
    var url = URL.createObjectURL(blob);
    return BINDING.js_string_to_mono_string(url);
}
function createFileURL(fileContent) {
    var bytes = base64ToArrayBuffer(fileContent);
    var blob = new Blob([bytes], { type: 'image/png' });
    var url = URL.createObjectURL(blob);
    return url;
}
function base64ToArrayBuffer(base64) {
    var binaryString = window.atob(base64);
    var binaryLen = binaryString.length;
    var bytes = new Uint8Array(binaryLen);
    for (var i = 0; i < binaryLen; i++) {
        var ascii = binaryString.charCodeAt(i);
        bytes[i] = ascii;
    }
    return bytes;
}


var boardRendererInstance;
function initBoardRendererInstance(instance) {
    boardRendererInstance = instance;
    window.requestAnimationFrame(redraw);
}
function redraw() {
    boardRendererInstance.invokeMethodAsync('InvokeRedrawRequestedAsync');
    window.requestAnimationFrame(redraw);
}

function redrawAllImages(divCanvasId, imgList) {
    clearMapCanvas(divCanvasId);
    for (var i = 0; i < imgList.length; i++) {
        redrawImage(divCanvasId, imgList[i].ref, imgList[i].x, imgList[i].y);
    }
}
function clearMapCanvas(divCanvasId) {
    var mapCanvas = document.getElementById(divCanvasId).getElementsByTagName('canvas')[0];
    var canvasW = mapCanvas.getBoundingClientRect().width;
    var canvasH = mapCanvas.getBoundingClientRect().height;

    var ctx = mapCanvas.getContext('2d');
    ctx.clearRect(0, 0, canvasW, canvasH);
    ctx.fillStyle = 'rgb(0,200,0)';
    ctx.fillRect(0, 0, canvasW, canvasH);
}
function redrawImage(divCanvasId, img, x, y) {
    var mapCanvas = document.getElementById(divCanvasId).getElementsByTagName('canvas')[0];;
    var ctx = mapCanvas.getContext('2d');
    ctx.drawImage(img, x, y);
}
