local function format_timestamp(inTimeStr)
    local inYear, inMonth, inDay, inHour, inMinute, inSecond, inMilliseconds =
        string.match(inTimeStr, '^(%d%d%d%d)-(%d%d)-(%d%d) (%d%d):(%d%d):(%d%d).(%d%d%d)$')

    local outTime = os.time({year=inYear, month=inMonth, day=inDay, hour=inHour, min=inMinute, sec=inSecond})

    return os.date("%Y-%m-%dT%H:%M:%S", outTime) .. string.format(".%sZ", inMilliseconds)
end

function convert_time_field(tag, timestamp, record)
    -- To "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'"
    local new_record = record
    new_record["localTime"] = format_timestamp(record["localTime"])
    return 2, timestamp, new_record
end
