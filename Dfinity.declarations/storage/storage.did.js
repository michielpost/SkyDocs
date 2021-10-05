export const idlFactory = ({ IDL }) => {
  const Name = IDL.Text;
  const Data = IDL.Text;
  const DataList = IDL.Service({
    'insert' : IDL.Func([Name, Data], [], []),
    'lookup' : IDL.Func([Name], [IDL.Opt(Data)], []),
    'whoami' : IDL.Func([], [IDL.Text], []),
  });
  return DataList;
};
export const init = ({ IDL }) => { return []; };
