import sys, os, time, timeit
import xml.etree.cElementTree as ET
from xml.etree.cElementTree import Element

from libc.stdio cimport *
 
cdef extern from "stdio.h":
    FILE *fopen(const char *, const char *)
    int fclose(FILE *)
    int fputs(const char* str, FILE* stream)


def getFilteredElement(mainFileName):
    context = iter(ET.iterparse(mainFileName, events=('start', 'end')))
    _, root = next(context)
    
    cdef int i = 0
    cdef float timeObjectProcessing = 0
    cdef float timeIterator = 0
    cdef float startIterator = time.process_time()
    for event, elem in context:
        endIterator = time.process_time()
        timeIterator += endIterator - startIterator

        i += 1
        if event == 'end' and elem.tag == "Object" and int(elem.get('AOLEVEL')) <= int('7'): # cities (level 4)
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


def manualToStr1(elem):
    res = ''
    res += '<' + str(elem.tag) + ' FORMALNAME="' + str(elem.get('FORMALNAME')) + '" AOLEVEL="' + str(elem.get('AOLEVEL')) + '" PLAINCODE="' + str(elem.get('PLAINCODE')) + '" />'
    return res.encode()


def manualToStr2(elem):
    formatStr = '<{} FORMALNAME="{}" AOLEVEL="{}" PLAINCODE="{}" />'
    resStr = formatStr.format(str(elem.tag),str(elem.get('FORMALNAME')),str(elem.get('AOLEVEL')),str(elem.get('PLAINCODE')))
    return resStr.encode()


def run(mainFileName, step):
    cdef int index = 0
    cdef bytes filename
    cdef char* cFilename
    cdef bytes lineStr
    cdef char* cLineStr
    cdef FILE* cFile
    cdef float timeFileIO = 0
    cdef float timeStringinizingElem = 0
    for elem in getFilteredElement(mainFileName):
        if elem.tag == 'Object':
            index += 1
            if index % step == 1: # create new file every 'step' elements
                if index != 1: # close previous root tag
                    fputs(b"</AddressObjects>\n", cFile)
                    fclose(cFile)
                
                filename = format(mainFileName.replace(".xml", "") + "_filtered" + str(index // step + 1) + ".xml").encode()
                cFilename = filename

                cFile = fopen(cFilename, "wb")
                fputs(b"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n", cFile)
                fputs(b"<AddressObjects>\n", cFile)

            startStringinizingElem = time.process_time()

            #lineStr = ET.tostring(elem, encoding='utf-8') + b"\n"
            lineStr = manualToStr2(elem) + b"\n"
            cLineStr = lineStr

            endStringinizingElem = time.process_time()
            timeStringinizingElem += endStringinizingElem - startStringinizingElem


            startFileIO = time.process_time()

            fputs(cLineStr, cFile)

            endFileIO = time.process_time()
            timeFileIO += endFileIO - startFileIO
            
            if index % 10000 == 1: # print progress for every 10'000 objects
                print("Written filtered objects: " + str(index))
                print("*CPU timeStringinizingElem: " + str(timeStringinizingElem))
                print("*CPU timeFileIO: " + str(timeFileIO))
    

    print("== CPU timeStringinizingElem: " + str(timeStringinizingElem))
    print("== CPU timeFileIO: " + str(timeFileIO))
    fputs(b"</AddressObjects>", cFile)

    fclose(cFile)

    print("- Total written filtered objects: " + str(index))
