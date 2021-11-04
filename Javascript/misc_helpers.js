
jQuery.fn.any = function (filter) {
    for (i = 0; i < this.length; i++) {
        if (filter.call(this[i])) return true;
    }
    return false;
};


function ParseDate(input, format) {
    format = format || 'yyyy-mm-dd'; // default format
    var parts = input.match(/(\d+)/g),
        i = 0, fmt = {};
    // extract date-part indexes from the format
    format.replace(/(yyyy|dd|mm)/g, function (part) { fmt[part] = i++; });

    return new Date(parts[fmt['yyyy']], parts[fmt['mm']] - 1, parts[fmt['dd']]);
}


function DateDiffInDays(endDate, startDate) {
    var _MS_PER_DAY = 1000 * 60 * 60 * 24;

    // Discard the time and time-zone information.
    var utcStart = Date.UTC(startDate.getFullYear(), startDate.getMonth(), startDate.getDate());
    var utcEnd = Date.UTC(endDate.getFullYear(), endDate.getMonth(), endDate.getDate());

    return Math.floor((utcEnd - utcStart) / _MS_PER_DAY);
}


var startDate = $('input[id^="xxx"][id$=yyy]').sort(function (first, second) {
	return ParseDate(first.value, 'dd.mm.yyyy') - ParseDate(second.value, 'dd.mm.yyyy'); // asc
})[0].value;