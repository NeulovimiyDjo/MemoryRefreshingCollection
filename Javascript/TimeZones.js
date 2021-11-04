function DateTimeToString(dateTime) {
	var day = dateTime.getDate() + "";
	var month = (dateTime.getMonth() + 1) + "";
	var year = dateTime.getFullYear() + "";
	var hour = dateTime.getHours() + "";
	var minutes = dateTime.getMinutes() + "";
	var seconds = dateTime.getSeconds() + "";

	day = checkZero(day);
	month = checkZero(month);
	year = checkZero(year);
	hour = checkZero(hour);
	mintues = checkZero(minutes);
	seconds = checkZero(seconds);

	return day + "." + month + "." + year + " " + hour + ":" + minutes + ":" + seconds;

	function checkZero(data) {
		if (data.length == 1) {
			data = "0" + data;
		}
		return data;
	}
}

 $(document).ready(function () {
	var dateTimeStr = $('input[id$=_SomeTimeFieldTimeGMT0300]').val();
	var dateTimeLocal = DateTimeToString(new Date(dateTimeStr + ' GMT+0300'));
	$('input[id$=_SomeTimeFieldTimeGMT0300]').val(dateTimeLocal);
 }