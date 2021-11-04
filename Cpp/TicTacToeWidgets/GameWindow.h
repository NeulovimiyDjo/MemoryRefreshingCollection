#ifndef GAME_WINDOW_H
#define GAME_WINDOW_H

#include <QWidget>
#include <QGridLayout>
#include <QLabel>
#include <QPushButton>
#include "ImageButton.h"
#include <vector>

class GameWindow : public QWidget {

  Q_OBJECT

public:
  GameWindow(QWidget* parent = 0);

private slots:
  void handleMouseRelease(ImageButton* btn);
  void resetGame();

protected:
  void resizeEvent(QResizeEvent* e);

private:
  std::vector<ImageButton*> buttons;
  QGridLayout* m_grid;
  QLabel* m_label;
  QPushButton* m_button;
  int count{ 0 };
  bool over{ false };

  void finishGame(bool found_chain);
  void checkGame();
};

#endif // GAME_WINDOW_H
