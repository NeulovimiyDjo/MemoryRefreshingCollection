import sys, os, time, timeit, re, io
import xml.etree.cElementTree as ET
from xml.etree.cElementTree import Element


def getFilteredElement():
    context = iter(ET.iterparse(mainFileName, events=('start', 'end')))
    _, root = next(context)
    
    i = 0
    timeObjectProcessing = 0
    timeIterator = 0
    startIterator = time.process_time()
    for event, elem in context:
        endIterator = time.process_time()
        timeIterator += endIterator - startIterator

        i += 1
        if event == 'end' and elem.tag == 'Object' and int(elem.get('AOLEVEL')) <= int('7'): # cities (level 4)
            startObjectProcessing = time.process_time()
            
            newElem = Element('Object')
            newElem.set('FORMALNAME', str(elem.get('FORMALNAME')))
            newElem.set('AOLEVEL', str(elem.get('AOLEVEL')))
            newElem.set('PLAINCODE', str(elem.get('PLAINCODE')))
            
            root.clear()
            endObjectProcessing = time.process_time()
            timeObjectProcessing += endObjectProcessing - startObjectProcessing
            yield newElem
            
        if i % 100000 == 1: # print progress for every 100'000 objects
            print("Iterated initial objects: " + str(i))
    

        startIterator = time.process_time()

    print("== CPU timeObjectProcessing: " + str(timeObjectProcessing))
    print("== CPU timeIterator: " + str(timeIterator))
    print("- Total iterated initial objects: " + str(i))





patternAttrib1 = re.compile(r'FORMALNAME\w?=\w?"(?P<value>.*?)"')
patternAttrib2 = re.compile(r'AOLEVEL\w?=\w?"(?P<value>.*?)"')
patternAttrib3 = re.compile(r'(PLAINCODE|PLANCODE)\w?=\w?"(?P<value>.*?)"')
def manualParseElem(elemStr):
    elem = Element('Object')

    #try:
    elem.set('FORMALNAME', patternAttrib1.search(elemStr).group('value'))
    elem.set('AOLEVEL', patternAttrib2.search(elemStr).group('value'))
    elem.set('PLAINCODE', patternAttrib3.search(elemStr).group('value'))
    #except Exception:
        #print(elemStr)
        #raise
    

    return elem




def parseByET2(elemStr):
    f = io.StringIO(elemStr)
    tree = ET.parse(f)
    root = tree.getroot()

    return root




buffSize = 4096 * 16 # idk, best results on * 16, but more doesn't help
patternElem = re.compile(r'<.*?>')
def manualParse():
    f = open(mainFileName, 'rt', encoding='UTF-8')

    start = f.tell()

    f.seek(0, 2) # to end
    eof = f.tell()

    f.seek(start, 0) # to start

    prevBuff = f.read(buffSize)

    curr = f.tell()
    if curr >= eof: # short file got read in 1 step
        buff = prevBuff
        #parse elements from buff with regexp
        return

    i = 0
    time1 = 0
    time2 = 0
    time3 = 0
    start3 = time.process_time()
    while True:
        currBuff = f.read(buffSize)
        
        buff = prevBuff + currBuff # prepend previous not yet parsed part
        
        pos = 0
        #parse elements from buff with regexp
        iterator = patternElem.finditer(buff)

        end3 = time.process_time()
        time3 += end3 - start3

        start2 = time.process_time()
        for match in iterator:  
            end2 = time.process_time()
            time2 += end2 - start2


            start1 = time.process_time()

            pos = match.end() + 1 #get position of the end of last element

            elemStr = match.group()
            if elemStr[1 : len('Object')+1] != 'Object':
                continue


            #elem = ET.fromstring(elemStr)
            #elem = parseByET2(elemStr)
            elem = manualParseElem(elemStr)


            newElem = Element('Object')
            newElem.set('FORMALNAME', str(elem.get('FORMALNAME')))
            newElem.set('AOLEVEL', str(elem.get('AOLEVEL')))
            newElem.set('PLAINCODE', str(elem.get('PLAINCODE')))

            end1 = time.process_time()
            time1 += end1 - start1

            i += 1
            if i % 10000 == 1:
                print("*CPU parseElements: " + str(time1))
                print("*CPU iterateMatches: " + str(time2))
                print("*CPU buffersManipulation: " + str(time3))
            if i // 100002 == 1:
                # Written filtered objects: 100001
                # == CPU parseElements: 1.1875
                # == CPU iterateMatches: 2.015625
                # == CPU buffersManipulation: 0.71875
                # == CPU timeStringinizingElem: 0.703125
                # == CPU timeFileIO: 0.171875
                eof = 1 #debug only first 100'000
                break
            yield newElem

            start2 = time.process_time()
        
        start3 = time.process_time()

        prevBuff = buff[pos:] # not yet parsed part, take it into account in next iteration
        curr = f.tell()
        if curr >= eof:
            break

    print("== CPU parseElements: " + str(time1))
    print("== CPU iterateMatches: " + str(time2))
    print("== CPU buffersManipulation: " + str(time3))


def manualToStr1(elem):
    res = ''
    res += '<' + str(elem.tag) + ' FORMALNAME="' + str(elem.get('FORMALNAME')) + '" AOLEVEL="' + str(elem.get('AOLEVEL')) + '" PLAINCODE="' + str(elem.get('PLAINCODE')) + '" />'
    return res.encode()


def manualToStr2(elem):
    formatStr = '<{} FORMALNAME="{}" AOLEVEL="{}" PLAINCODE="{}" />'
    resStr = formatStr.format(str(elem.tag),str(elem.get('FORMALNAME')),str(elem.get('AOLEVEL')),str(elem.get('PLAINCODE')))
    return resStr.encode()


def main():
    index = 0
    filename = None
    f = None
    timeFileIO = 0
    timeStringinizingElem = 0
    for elem in manualParse(): #for elem in getFilteredElement():
        if elem.tag == 'Object':
            index += 1
            if index % step == 1: # create new file every 'step' elements
                if index != 1: # close previous root tag
                    f.write(b"</AddressObjects>\n")
                
                filename = format(mainFileName.replace(".xml", "") + "_filtered" + str(index // step + 1) + ".xml")
                f = open(filename, 'wb')
                f.write(b"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n")
                f.write(b"<AddressObjects>\n")

            startStringinizingElem = time.process_time()
            
            #lineStr = ET.tostring(elem, encoding='utf-8') + b"\n"
            lineStr = manualToStr2(elem) + b"\n"

            endStringinizingElem = time.process_time()
            timeStringinizingElem += endStringinizingElem - startStringinizingElem


            startFileIO = time.process_time()

            f.write(lineStr)

            endFileIO = time.process_time()
            timeFileIO += endFileIO - startFileIO
            
            if index % 10000 == 1: # print progress for every 10'000 objects
                f.flush()
                os.fsync(f)
                print("Written filtered objects: " + str(index))
                print("*CPU timeStringinizingElem: " + str(timeStringinizingElem))
                print("*CPU timeFileIO: " + str(timeFileIO))
            

    print("== CPU timeStringinizingElem: " + str(timeStringinizingElem))
    print("== CPU timeFileIO: " + str(timeFileIO))
    f.write(b"</AddressObjects>")

    print("- Total written filtered objects: " + str(index))


if __name__ == "__main__":
    # usage test: filter_and_split_fias_xml.py fias_test.xml 8
    # usage real: filter_and_split_fias_xml.py fias.xml 9000000
    mainFileName = sys.argv[1]
    step = int(sys.argv[2])


    startCPU = time.process_time()
    startSystem = timeit.default_timer()

    main()

    endCPU = time.process_time()
    endSystem = timeit.default_timer()


    print("-- Total CPU time used (seconds): " + str(endCPU - startCPU))
    print("-- Total System time used (seconds): " + str(endSystem - startSystem))
