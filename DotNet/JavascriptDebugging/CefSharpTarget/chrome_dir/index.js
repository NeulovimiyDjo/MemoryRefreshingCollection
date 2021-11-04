function sleep(ms) {
	return new Promise(resolve => setTimeout(resolve, ms));
}
sleep(30000).then(function () {
	//debugger;
	document.getElementById('p1').textContent = 999;
});

//sleep(30000).then(function () {
	var count = 1;

	var func = function () {
		var two;
		//debugger;
		document.getElementById('p1').textContent = count;
		myWorker.postMessage(count);
		count++;
		//throw Error("exMsg11");
		return two + 2;
	};

	//var result = func();


	function worker_function() {
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
	}
	//var myWorker = new Worker(URL.createObjectURL(new Blob(["(" + worker_function.toString() + ")()"], { type: 'text/javascript' })));
	var myWorker = new Worker('worker.js');
	myWorker.onmessage = function (e) {
		document.getElementById('p2').textContent = e.data;
	}

	document.getElementById('p1').textContent = 'YAYAYAYA111111';
	document.getElementById('p2').textContent = 'YAYAYAYA222222';
	document.getElementById('p1').addEventListener('mouseover', func, false);

//});
