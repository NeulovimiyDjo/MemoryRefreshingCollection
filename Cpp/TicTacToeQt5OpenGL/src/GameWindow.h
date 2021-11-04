#ifndef GAME_WINDOW_H
#define GAME_WINDOW_H

#include "GameEngine.h"

#include <QWidget>

class FieldWidget;
class QPushButton;
class QSpinBox;


class GameWindow : public QWidget {
  Q_OBJECT

public:
  GameWindow();

signals:
  void newGameRequested(int dim_x, int dim_y, int win_size);

private:
  void emitNewGameRequest();
  void positionWidgets();

  GameEngine m_game_engine;

  FieldWidget* m_field_widget;
  
  QSpinBox* m_edit_dim_x;
  QSpinBox* m_edit_dim_y;
  QSpinBox* m_edit_win_size;

  QPushButton* m_button_reset;
};

#endif // GAME_WINDOW_H
