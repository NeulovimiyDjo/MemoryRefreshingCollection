#include <gtest/gtest.h>

#include <gmock/gmock.h>

#include "lib.cpp"

TEST(SquareRootTest, PositiveNos) {
	A a;
  EXPECT_THAT(a.x, testing::Eq(int(4)));
}
