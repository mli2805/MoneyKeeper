﻿namespace Keeper.Utils.DiagramDomainModel
{
	public enum DiagramMode
	{
		BarHorizontal   = 1, // столбцы разных серий для одной даты находятся рядом, могут быть отрицательные
		BarVertical     = 2, // столбцы для одной даты ставятся один на один, могут быть отрицательные
		BarVertical100  = 3, // столбцы для одной даты ставятся один на один и сумма считается за 100%, не должно быть отрицательных
		Lines           = 101, // одна и более линий одновременно
		SeparateLines   = 102  // линии показываются поочередно (большая разница значений по осям)
	}
}