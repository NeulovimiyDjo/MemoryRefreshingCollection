#include "ImageButton.h"

#include <QPainter>

ImageButton::ImageButton(QQuickItem* parent) : QQuickPaintedItem(parent) {
  setAcceptedMouseButtons(Qt::AllButtons);
  m_is_pressed = false;
  m_is_in_victory_chain = false;
  m_is_game_over = false;
}

void ImageButton::setSource(const QString& source) {
  if (m_source != source && source != "") {
    m_source = source;
    update();
  }
}

void ImageButton::setIsInVictoryChain(const bool is_in_victory_chain) {
  if (m_is_in_victory_chain != is_in_victory_chain) {
    m_is_in_victory_chain = is_in_victory_chain;
    update();
  }
}

void ImageButton::setIsGameOver(const bool is_game_over) {
  if (m_is_game_over != is_game_over) {
    m_is_game_over = is_game_over;
    update();
  }
}

void ImageButton::mousePressEvent(QMouseEvent* e) {
  m_is_pressed = true;
  update();
}

void ImageButton::mouseReleaseEvent(QMouseEvent* e) {
  m_is_pressed = false;
  update();
  emit clicked();
}

void ImageButton::paint(QPainter* painter) {
  QColor color;
  int pen_half_size;

  if (m_is_in_victory_chain) {
    color = QColor(0, 205, 0, 120);
    pen_half_size = std::round(boundingRect().width()/20);
    if (pen_half_size == 0) pen_half_size = 2;
  }
  else {
    if (m_is_pressed && !m_is_game_over) {
      color = QColor(205, 205, 205, 205);
      pen_half_size = std::round(boundingRect().width()/20);
      if (pen_half_size == 0) pen_half_size = 2;
    }
    else {
      color = QColor(225, 225, 225, 225);
      pen_half_size = std::round(boundingRect().width()/40);
      if (pen_half_size == 0) pen_half_size = 1;
    }
  }

  painter->setPen(QPen(color, pen_half_size*2));
  painter->setRenderHints(QPainter::Antialiasing, true);
  painter->drawRect(boundingRect().adjusted(pen_half_size, pen_half_size, -pen_half_size, -pen_half_size));

  if (m_is_game_over)
    painter->setOpacity(0.5);
  else
    painter->setOpacity(1);

  int margin = std::round(boundingRect().width()/20)*2;
  if (margin == 0) margin = 2;
  painter->drawImage(QRect(margin, margin, width()-margin*2, height()-margin*2), QImage(m_source));
}

