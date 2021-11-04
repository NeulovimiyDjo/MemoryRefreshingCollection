import csv

def read_rows_from_csv(filename):
  rows = []
  with open(filename, encoding='utf-8-sig', mode='r') as csv_file:
    csv_reader = csv.DictReader(csv_file, delimiter=';')
    for row in csv_reader:
      rows.append(row)
  return rows

rows = read_rows_from_csv('xxx.csv')
for row in rows:
    val = f"""
    xxx {row['column2name']}
    """


list = []
with open('yyy.csv', encoding='utf-8', mode='r') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=';')
    line_count = 0
    for row in csv_reader:
        if line_count == 0:
            line_count += 1
        else:
            list.append({ 'key': row[0], 'value': row[1] })
            line_count += 1
    print(f'Read {line_count} lines')


def read_csv_file(file_name):
    values = {}
    with open(file_name, encoding='utf-8', mode='r') as csv_file:
        csv_reader = csv.reader(csv_file, delimiter=',')
        line_count = 0
        for row in csv_reader:
            if line_count == 0:
                line_count += 1
            else:
                values[f'{row[1]}'] = row[2]
                line_count += 1
        print(f'Read {line_count} lines')
    return values


def convert(result_file_name, values):
    with open(result_file_name, encoding='utf-8', mode='w', newline='') as csv_file:
        csv_writer = csv.writer(csv_file, delimiter=';', quotechar='"', quoting=csv.QUOTE_MINIMAL)
        csv_writer.writerow(['column1name', 'column2name', 'column3name'])
        line_count = 0
        for list_item in list:
            if list_item["key"] in values:
                csv_writer.writerow([list_item["key"], list_item["column2name"], values[list_item["key"]]])
                line_count += 1
            else:
                print(f'list_item {list_item["key"]} is missing in values.')
        print(f'Created converted list_items file with {line_count} known values.')
