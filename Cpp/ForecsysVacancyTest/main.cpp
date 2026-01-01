/*
Задание:

Есть txt файл в формате



user_id;count;country

11231;6;Russia

11232;1;Ukraine

1122;1;Ukraine

Нужно вывести статистику по country:

country;sum(count) (сумма по count);count_uniq(user_id) (число уникальных user_id для country)

Предусмотреть устойчивость программы к ошибке формата, если строка не соответствует формату, она пропускается.

Язык на выбор C++, C#.

Плюсом будет, если задание будет решено без использования дополнительных библиотек структур данных и алгоритмов
(то есть с базовыми типами, циклами и массивами. Словари, множества, хэш-таблицы уже считаются дополнительными структурами данных).
*/

/*
Основываясь на предложении не использовать хэш-таблицы и использовать циклы и массивы я осмеюсь предположить
что был намек на ввод данных в ин-мемори массив и его быструю сортировку (написанную вручну)
и дальнейший вывод данных из этого отсортированного массива ведя счетчики для текущей страны
(одинаковые страны идут вместе т.к. массив отсортирован). В коде ниже было принято предположение о том
что во всем множестве валидных входных записей нет одинаковых пар {user_id, country}
(Если это не так то можно было бы отсортировать по user_id каждый подмассив с одинаковых стран
(а.к.а. отсортировать по 2му приоритету/полю) и инкрементировать счетчик user_id только при изменении user_id
а не каждый раз (см. функцию getStatisticsAndPrint)).

Это дает O(n) сложность по памяти и скорости на чтение коллекции в память с диска +
O(n*log(n)) сложность по скорости для сортировки массива.
Используя хэш-таблицы (в том числе реализованные вручную) можно было бы снизить до
O(n) по скорости на чтение с диска + O(m) по памяти для хэш-таблицы,
где m = количество разных стран.

Я позволил себе использовать вектор из библиотечных функций. Используя билт-ин массивы вместо вектора
можно было бы либо прочесть файл 2 раза(1й раз чтобы определить его размер)
либо динамически перевыделять память в геометрической прогрессии как в реализации вектора.

Так же я принял допущение что записи вроде "33;3;;Test;" валидны т.к. ";Test;"
это валидное значение строки, и использовал стримы и стринг. Не бейте :)
*/

#include <cstdlib>
#include <fstream>
#include <iostream>
#include <string>
#include <sstream>
#include <vector>

// represents each valid line in input file
struct InitialRecord {
  int id;
  int count;
  std::string country;
};


//Start-----------------implementation of qucksort for vector<Elem> by country---------------Start//
struct EndStart {
  int firstEnd;
  int secondStart;
};

EndStart reorder(std::vector<InitialRecord>& arr, int start, int end, const std::string& pivot) {
  int left = start;
  int right = end;

  while (left < right) {
    while (arr[left].country < pivot) ++left;
    while (arr[right].country > pivot) --right;

    if (left <= right) {
      InitialRecord tmp = arr[left];
      arr[left] = arr[right];
      arr[right] = tmp;

      ++left;
      --right;
    }
  }

  return EndStart{ right, left };
}

void quicksort(std::vector<InitialRecord>& arr, int start, int end) {
  std::string pivot = arr[start + (end - start) / 2].country;

  EndStart es = reorder(arr, start, end, pivot);

  if (es.firstEnd > start)
    quicksort(arr, start, es.firstEnd);
  if (es.secondStart < end)
    quicksort(arr, es.secondStart, end);
}


void sortByCountry(std::vector<InitialRecord>& initialArray) {
  quicksort(initialArray, 0, initialArray.size() - 1);
}
//End-----------------implementation of qucksort for vector<InitialRecord> by country---------------End//


// read input file line by line and if line is valid adds it to in-memory array of records
std::vector<InitialRecord> readAllRecords(std::ifstream& ifs) {
  std::vector<InitialRecord> records;

  std::string line;
  while (std::getline(ifs, line)) {
    std::istringstream iss(line);

    int id, count;
    std::string country;
    char semicolon1, semicolon2;
    if (iss >> id >> semicolon1 >> count >> semicolon2 >> country) {
      if (semicolon1 != ';' || semicolon2 != ';') continue;

      records.emplace_back(InitialRecord{ id, count, country });
    } else {
      continue;
    }
  }

  return records;
}


// walk through sorted by country collection of records and aggregate count and userCount while country is the same,
// if country changes print aggregated values and start aggregation count and userCount for new country
// then print aggregated values for last country in collection after the end of the loop
void getStatisticsAndPrint(std::vector<InitialRecord> records) {
  std::string country;
  int count;
  int userCount;

  for (int i = 0; i < records.size(); ++i) {
    const InitialRecord& currElem = records[i];

    if (currElem.country != country) {
      if (country != "") {
        std::cout << country << ';' << count << ';' << userCount << '\n';
      }

      country = currElem.country;
      count = currElem.count;
      userCount = 1;
    } else {
      count += currElem.count;


      // the assupmption was taken that {user_id, country} pairs are unique in records
      // which means each user_id is different for one country
      userCount += 1;
    }
  }

  std::cout << country << ';' << count << ';' << userCount << '\n';
}



void printStatistics(std::ifstream& ifs) {
  std::vector<InitialRecord> records = readAllRecords(ifs);
  sortByCountry(records);

  getStatisticsAndPrint(records);
}



int main(int argc, char* argv[]) {
  if (argc != 2) {
    std::cout << "Error: wrong arguments\n";
    std::cout << "Usage: " << "<executable-name>" << " <input-filename>\n";

    std::cin.get();
    return EXIT_FAILURE;
  }


  std::ifstream ifs(argv[1]);
  if (ifs.fail()) {
    std::cout << "Error: failed to open " << argv[1] << "\n";
    std::cout << "Usage: " << "<executable-name>" << " <input-filename>\n";

    std::cin.get();
    return EXIT_FAILURE;
  }


  printStatistics(ifs);


  std::cin.get();
  return EXIT_SUCCESS;
}