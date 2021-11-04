import sys
import hashlib
import requests

def checkPassword(unicodePwd):
  pwd = unicodePwd.encode('utf-8')

  pHash = hashlib.sha1(pwd).hexdigest()
  first5 = pHash[:5].upper()
  last35 = pHash[5:].upper()

  response = requests.get('https://api.pwnedpasswords.com/range/' + first5)
  stringRes = response.content.decode('utf-8')
  arr = stringRes.split('\r\n')

  for line in arr:
    end = line.split(':')[0]
    count = line.split(':')[1]
    if end == last35:
      return count

  return 0


def main():
  try:
    while True:
      count = checkPassword(input('Enter password to check: '))
      print('found ' + str(count) + ' passwords')
  
  except KeyboardInterrupt:
    print('exiting on ^C')
    sys.exit()


if __name__ == '__main__':
  main()
