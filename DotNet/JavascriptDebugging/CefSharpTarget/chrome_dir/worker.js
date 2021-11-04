onmessage = function (e) {
    postMessage(e.data); // send back same
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function runPeriodicLoop() {
    var cnt = 0;
    while (true) {
        postMessage(cnt);
        await sleep(2000);
        cnt++;
        debugger;
    }
}

runPeriodicLoop();