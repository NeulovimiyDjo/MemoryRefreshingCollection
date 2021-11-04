#include "ImageButton.h"
#include <QPainter>

ImageButton::ImageButton(QWidget* parent) :
  QFrame(parent), clear{ ":/clear.png" }, cross{ ":/cross.png" }, nought{ ":/nought.png" }
{
  setFrameStyle(QFrame::Box | QFrame::Raised);
  setLineWidth(4);

  label = new QLabel(this);
  label->setMargin(8);
}

void ImageButton::mousePressEvent(QMouseEvent* e) {
  setFrameStyle(QFrame::Box | QFrame::Sunken);
}

void ImageButton::mouseReleaseEvent(QMouseEvent* e) {
  setFrameStyle(QFrame::Box | QFrame::Raised);
  emit mouseReleased(this);
}

void ImageButton::resizeEvent(QResizeEvent* e) {
  label->setFixedSize(width(), height());
  if (status == StatusType::clear)
    label->setPixmap(clear.scaled(width()-16, height()-16));
  else if (status == StatusType::cross)
    label->setPixmap(cross.scaled(width() - 16, height() - 16));
  else if (status == StatusType::nought)
    label->setPixmap(nought.scaled(width() - 16, height() - 16));
}

void ImageButton::setStatus(StatusType st) {
  status = st;
  if (st == ImageButton::StatusType::clear)
    label->setPixmap(clear.scaled(width() - 16, height() - 16));
  else if (st == ImageButton::StatusType::cross)
    label->setPixmap(cross.scaled(width() - 16, height() - 16));
  else if (st == ImageButton::StatusType::nought)
    label->setPixmap(nought.scaled(width() - 16, height() - 16));
}

void ImageButton::setAlpha(double alpha) {
  QImage image;
  if (status == ImageButton::StatusType::clear)
    return;
  else if (status == ImageButton::StatusType::cross)
    image.load(":/cross.png");
  else if (status == ImageButton::StatusType::nought)
    image.load(":/nought.png");

  QPainter p;
  p.begin(&image);
  p.setCompositionMode(QPainter::CompositionMode_DestinationIn);
  p.fillRect(image.rect(), QColor(0, 0, 0, std::round(alpha*255)));
  p.end();
  label->setPixmap(QPixmap::fromImage(image).scaled(width() - 16, height() - 16));
}

