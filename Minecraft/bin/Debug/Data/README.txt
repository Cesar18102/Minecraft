conf.ini format - items sources info

 Name |  Type   | Length | Description
---------------------------------------------
SNLen |  UInt32 |   2    | Source name length
---------------------------------------------
SrcN  |  String | SNLen  | Source name
---------------------------------------------
SLen  |  UInt32 |   2    | Source length
---------------------------------------------
DirSrc|  String |  SLen  | Source dir path
---------------------------------------------
SCLen |  UInt32 |   2    | Source config length
---------------------------------------------
SConf |  String |  SCLen  | Source config
