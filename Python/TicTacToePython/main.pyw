from tkinter import *

def setWinnerInfoString(got_victory_chain):
  global winner_info_str
  if got_victory_chain:
    if moves_count%2 == 0:
      winner_info_str = "Noughts won"
    elif moves_count%2 == 1:
      winner_info_str = "Crosses won"
  else:
    winner_info_str = "Draw"

def isGameOver():
  # check Left->Right
  for i in range(0,3):
    found_chain = True
    start_state = buttons_list[i*3]['state']
    if start_state == states['Clear']:
      found_chain = False
    else:
      for j in range(1,3):
        if buttons_list[i*3+j]['state'] != start_state:
          found_chain = False
    if found_chain:
      setWinnerInfoString(True)
      return True

  # check Top->Bottom
  for i in range(0,3):
    found_chain = True
    start_state = buttons_list[i]['state']
    if start_state == states['Clear']:
      found_chain = False
    else:
      for j in range(1,3):
        if buttons_list[i+j*3]['state'] != start_state:
          found_chain = False
    if found_chain:
      setWinnerInfoString(True)
      return True

  # check TopLeft->BottomRight
  for i in range(0,1):
    found_chain = True
    start_state = buttons_list[i*3]['state']
    if start_state == states['Clear']:
      found_chain = False
    else:
      for j in range(1,3):
        if buttons_list[(i+j)*3+j]['state'] != start_state:
          found_chain = False
    if found_chain:
      setWinnerInfoString(True)
      return True

  # check TopRight->BottomLeft
  for i in range(0,1):
    found_chain = True
    start_state = buttons_list[i*3+2]['state']
    if start_state == states['Clear']:
      found_chain = False
    else:
      for j in range(1,3):
        if buttons_list[(i+j)*3+2-j]['state'] != start_state:
          found_chain = False
    if found_chain:
      setWinnerInfoString(True)
      return True

  if moves_count == 9:
    setWinnerInfoString(False)
    return True
  else:
    return False

def finishGame():
  global game_over
  game_over = True
  #print("GAME OVER")
  game_info.config(text = "Moves made: {}\nGame over\n{}".format(moves_count, winner_info_str))
  for i in range(0,9):
    buttons_list[i]['button'].config(state = DISABLED)
    #buttons_list[i]['button'].config(command = '')

def onButtonPressed(n):
  global moves_count, game_over
  if not game_over and buttons_list[n]['state'] == states['Clear']:
    if moves_count%2 == 0:
      buttons_list[n]['button'].config(image = img_cross)
      buttons_list[n]['state'] = states['Cross']
      moves_count += 1
    elif moves_count%2 == 1:
      buttons_list[n]['button'].config(image = img_nought)
      buttons_list[n]['state'] = states['Nought']
      moves_count += 1
    game_info.config(text = "Moves made: {}".format(moves_count))
    if isGameOver():
      finishGame()

def initGame():
  for i in range(0,9):
    btn = Button(root, image = img_clear, command = lambda i = i: onButtonPressed(i))
    btn.grid(row = i//3, column = i%3)
    buttons_list.append({'state':states['Clear'], 'button':btn})

def resetGame():
  global game_over
  global moves_count

  game_over = False
  moves_count = 0
  game_info.config(text = "Moves made: {}\n".format(moves_count))

  for i in range(0,9):
    buttons_list[i]['state'] = states['Clear']
    buttons_list[i]['button'].config(image = img_clear)
    buttons_list[i]['button'].config(state = NORMAL)

states = { 'Clear':0, 'Cross':1, 'Nought':2 }

root = Tk()
root.title("TicTacToe Python")
root.geometry("640x480+100+100")

game_over = False
winner_info_str = ""
moves_count = 0

buttons_list = list()

img_clear = PhotoImage(file = "clear.png")
img_cross = PhotoImage(file = "cross.png")
img_nought = PhotoImage(file = "nought.png")

frame = Frame(root)
frame.grid(row = 0, column = 3, rowspan = 2, padx = 100)

game_info = Label(frame, text = "Moves made: 0", justify = LEFT, height = 8, anchor = "n", pady = 30)
game_info.grid(row = 0, column = 0)
reset_button = Button(frame, command = lambda: resetGame(), text = "Reset", width = 15)
reset_button.grid(row = 1, column = 0)

initGame()
root.mainloop()
