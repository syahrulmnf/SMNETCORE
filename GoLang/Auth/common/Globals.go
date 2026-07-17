package common

type Globals struct {
	GeneralTenant string
	GenericDB     string
}

var GlobalsData Globals

func Init() {
	GlobalsData = Globals{
		GeneralTenant: "_Clients",
		GenericDB:     "Generic",
	}
}
