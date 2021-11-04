import sys, time, timeit
import filter_and_split_fias_xml


def main():
    filter_and_split_fias_xml.run(mainFileName, step)


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