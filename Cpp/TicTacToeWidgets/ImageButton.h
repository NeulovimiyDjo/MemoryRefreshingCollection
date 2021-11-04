#ifndef IMAGE_BUTTON_H
#define IMAGE_BUTTON_H

#include <QWidget>
#include <QFrame>
#include <QLabel>

class ImageButton : public QFrame {

  Q_OBJECT

public:
  ImageButton(QWidget* parent = 0);
  enum class StatusType { clear, cross, nought };
  StatusType getStatus() { return status; }
  void setStatus(StatusType st);
  void setAlpha(double alpha);

signals:
  void mouseReleased(ImageButton* btn);

protected:
  void mousePressEvent(QMouseEvent* e);
  void mouseReleaseEvent(QMouseEvent* e);
  void resizeEvent(QResizeEvent* e);

private:
  QLabel* label;
  QPixmap clear;
  QPixmap cross;
  QPixmap nought;
  StatusType status{ StatusType::clear };
};

#endif // IMAGE_BUTTON_H
