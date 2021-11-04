#ifndef IMAGE_BUTTON_H
#define IMAGE_BUTTON_H

#include <QQuickPaintedItem>

class ImageButton : public QQuickPaintedItem {
  Q_OBJECT
  Q_PROPERTY(QString source WRITE setSource)
  Q_PROPERTY(bool isInVictoryChain WRITE setIsInVictoryChain)
  Q_PROPERTY(bool isGameOver WRITE setIsGameOver)

public:
  ImageButton(QQuickItem* parent = nullptr);

  void setSource(const QString& source);
  void setIsInVictoryChain(const bool is_in_victory_chain);
  void setIsGameOver(const bool is_game_over);

  void paint(QPainter* painter);

signals:
  void clicked();

protected:
  void mousePressEvent(QMouseEvent* e);
  void mouseReleaseEvent(QMouseEvent* e);

private:
  QString m_source;
  bool m_is_pressed;
  bool m_is_in_victory_chain;
  bool m_is_game_over;
};

#endif // IMAGE_BUTTON_H
