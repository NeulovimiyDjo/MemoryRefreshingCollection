#include "GameWindow.h"
#include "FieldWidget.h"

#include <QLabel>
#include <QFormLayout>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QSpinBox>
#include <QPushButton>


GameWindow::GameWindow() {
  m_field_widget = new FieldWidget;

  m_edit_dim_x = new QSpinBox;
  m_edit_dim_x->setRange(3, 100);
  m_edit_dim_x->setValue(3);

  m_edit_dim_y = new QSpinBox;
  m_edit_dim_y->setRange(3, 100);
  m_edit_dim_y->setValue(3);

  m_edit_win_size = new QSpinBox;
  m_edit_win_size->setRange(3, 30);
  m_edit_win_size->setValue(3);

  m_button_reset = new QPushButton("New Game");



  connect(m_field_widget, &FieldWidget::squareClicked, &m_game_engine, &GameEngine::makeMove);

  connect(&m_game_engine, &GameEngine::moveMade, m_field_widget, &FieldWidget::setSquare);
  connect(&m_game_engine, &GameEngine::gameOver, m_field_widget, &FieldWidget::finishGame);

  connect(m_button_reset, &QPushButton::clicked, this, &GameWindow::emitNewGameRequest);
  connect(this, &GameWindow::newGameRequested, &m_game_engine, &GameEngine::newGame);

  connect(&m_game_engine, &GameEngine::gameCreated, m_field_widget, &FieldWidget::newGame);


  connect(m_field_widget, &FieldWidget::glInitialized, this, &GameWindow::emitNewGameRequest);


  positionWidgets();
}


void GameWindow::positionWidgets() {
  QFormLayout* form_layout = new QFormLayout;

  QLabel* l_x = new QLabel("X: ");
  m_edit_dim_x->setButtonSymbols(QAbstractSpinBox::NoButtons);
  form_layout->addRow(l_x, m_edit_dim_x);

  QLabel* l_y = new QLabel("Y: ");
  m_edit_dim_y->setButtonSymbols(QAbstractSpinBox::NoButtons);
  form_layout->addRow(l_y, m_edit_dim_y);

  QLabel* l_w = new QLabel("W: ");
  m_edit_win_size->setButtonSymbols(QAbstractSpinBox::NoButtons);
  form_layout->addRow(l_w, m_edit_win_size);



  QVBoxLayout* game_control_layout = new QVBoxLayout;
  game_control_layout->setMargin(0);

  game_control_layout->addStretch(3);

  game_control_layout->addLayout(form_layout);

  game_control_layout->addStretch(1);

  game_control_layout->addWidget(m_button_reset);

  game_control_layout->addStretch(5);



  QHBoxLayout* main_layout = new QHBoxLayout;
  main_layout->setMargin(0);

  m_field_widget->setSizePolicy(QSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding));
  main_layout->addWidget(m_field_widget);

  main_layout->addLayout(game_control_layout);



  setLayout(main_layout);
}


void GameWindow::emitNewGameRequest() {
  int dim_x = m_edit_dim_x->value();
  int dim_y = m_edit_dim_y->value();
  int win_size = m_edit_win_size->value();

  emit newGameRequested(dim_x, dim_y, win_size);
}
