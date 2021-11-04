#include "GameWindow.h"
#include <QVBoxLayout>

GameWindow::GameWindow(QWidget* parent) : QWidget(parent) {
  m_grid = new QGridLayout();

  auto grid = new QGridLayout();
  grid->setSpacing(0);
  for (int i = 0; i < 3; ++i)
    for (int j = 0; j < 3; ++j) {
      auto ib = new ImageButton();
      grid->addWidget(ib, i, j);
      connect(ib, &ImageButton::mouseReleased, this, &GameWindow::handleMouseRelease);
      buttons.push_back(ib);
    }

  auto vbox = new QVBoxLayout();
  vbox->addStretch(2);
  m_label = new QLabel("Moves made: 0");
  m_label->setAlignment(Qt::AlignHCenter | Qt::AlignTop);
  vbox->addWidget(m_label);
  vbox->setStretchFactor(m_label, 2);
  vbox->addStretch(1);
  m_button = new QPushButton("reset");
  vbox->addWidget(m_button);
  vbox->addStretch(1);

  connect(m_button, &QPushButton::released, this, &GameWindow::resetGame);

  m_grid->addLayout(grid, 1, 1);
  m_grid->addLayout(vbox, 1, 3);

  m_grid->setSpacing(0);
  m_grid->setMargin(0);
  m_grid->setRowMinimumHeight(0, 10);
  m_grid->setRowMinimumHeight(1, 210);
  m_grid->setRowMinimumHeight(2, 20);
  m_grid->setRowMinimumHeight(3, 0);
  m_grid->setColumnMinimumWidth(0, 10);
  m_grid->setColumnMinimumWidth(1, 210);
  m_grid->setColumnMinimumWidth(2, 10);
  m_grid->setColumnMinimumWidth(3, 70);
  m_grid->setColumnMinimumWidth(4, 20);
  m_grid->setColumnMinimumWidth(5, 0);

  int w = width();
  int h = height();
  int x = (480 * w > 640 * h) ? 480 * w / h - 640 : 0;
  int y = (480 * w > 640 * h) ? 0 : 640 * h / w - 480;

  m_button->setFixedHeight(m_button->width() / 2);
  QFont font = m_button->font();
  font.setPointSize(std::min(w * (640 - 220) / 640, h * (480 - 60) / 480) / 30);
  m_label->setFont(font);
  font.setPointSize(std::min(w * (640 - 220) / 640, h * (480 - 60) / 480) / 20);
  m_button->setFont(font);

  m_grid->setRowStretch(0, 20);
  m_grid->setRowStretch(1, 420);
  m_grid->setRowStretch(2, 40);
  m_grid->setRowStretch(3, y);
  m_grid->setColumnStretch(0, 20);
  m_grid->setColumnStretch(1, 420);
  m_grid->setColumnStretch(2, 20);
  m_grid->setColumnStretch(3, 140);
  m_grid->setColumnStretch(4, 40);
  m_grid->setColumnStretch(5, x);


  setLayout(m_grid);
}

void GameWindow::resizeEvent(QResizeEvent* e) {
  int w = width();
  int h = height();
  int x = (480 * w > 640 * h) ? 480 * w / h - 640 : 0;
  int y = (480 * w > 640 * h) ? 0 : 640 * h / w - 480;

  m_button->setFixedHeight(m_button->width() / 2);
  QFont font = m_button->font();
  font.setPointSize(std::min(w * (640-220)/640, h * (480-60)/480) / 30);
  m_label->setFont(font);
  font.setPointSize(std::min(w * (640 - 220) / 640, h * (480 - 60) / 480) / 20);
  m_button->setFont(font);

  m_grid->setRowStretch(0, 20);
  m_grid->setRowStretch(1, 420);
  m_grid->setRowStretch(2, 40);
  m_grid->setRowStretch(3, y);
  m_grid->setColumnStretch(0, 20);
  m_grid->setColumnStretch(1, 420);
  m_grid->setColumnStretch(2, 20);
  m_grid->setColumnStretch(3, 140);
  m_grid->setColumnStretch(4, 40);
  m_grid->setColumnStretch(5, x);
}

void GameWindow::handleMouseRelease(ImageButton* btn) {
  if (btn->getStatus() == ImageButton::StatusType::clear && !over) {
    if (count % 2 == 0) {
      btn->setStatus(ImageButton::StatusType::cross);
    }
    else if (count % 2 == 1) {
      btn->setStatus(ImageButton::StatusType::nought);
    }
    ++count;
    checkGame();
  }
}

void GameWindow::finishGame(bool found_chain) {
  over = true;

  if (found_chain && count % 2 == 1)
    m_label->setText(QString("Moves made: %1\nCrosses Won").arg(count));
  else if (found_chain && count % 2 == 0)
    m_label->setText(QString("Moves made: %1\nNoughts Won").arg(count));
  else
    m_label->setText(QString("Moves made: %1\nDraw").arg(count));

  for (auto b : buttons) {
    b->setAlpha(0.5);
  }
}

void GameWindow::checkGame() {
  ImageButton::StatusType start;
  bool found_chain;

  // check rows
  for (int i = 0; i < 3; ++i) {
    start = buttons[i * 3]->getStatus();
    if (start != ImageButton::StatusType::clear) {
      found_chain = true;
      for (int k = 1; k < 3; ++k) {
        if (buttons[i * 3 + k]->getStatus() != start)
          found_chain = false;
      }
      if (found_chain) {
        finishGame(true);
        return;
      }
    }
  }

  // check columns
  for (int j = 0; j < 3; ++j) {
    start = buttons[j]->getStatus();
    if (start != ImageButton::StatusType::clear) {
      found_chain = true;
      for (int k = 1; k < 3; ++k) {
        if (buttons[j + k * 3]->getStatus() != start)
          found_chain = false;
      }
      if (found_chain) {
        finishGame(true);
        return;
      }
    }
  }

  // check dioganal right+down
  start = buttons[0]->getStatus();
  if (start != ImageButton::StatusType::clear) {
    found_chain = true;
    for (int k = 1; k < 3; ++k) {
      if (buttons[k * 3 + k]->getStatus() != start)
        found_chain = false;
    }
    if (found_chain) {
      finishGame(true);
      return;
    }
  }

  // check dioganal left+down
  start = buttons[2]->getStatus();
  if (start != ImageButton::StatusType::clear) {
    found_chain = true;
    for (int k = 1; k < 3; ++k) {
      if (buttons[2 + k * 3 - k]->getStatus() != start)
        found_chain = false;
    }
    if (found_chain) {
      finishGame(true);
      return;
    }
  }
 
  if (count == 9) {
    finishGame(false);
    return;
  }

  m_label->setText(QString("Moves made: %1").arg(count));
}

void GameWindow::resetGame() {
  count = 0;
  over = false;
  m_label->setText(QString("Moves made: 0"));

  for (auto b : buttons) {
    b->setStatus(ImageButton::StatusType::clear);
  }
}
