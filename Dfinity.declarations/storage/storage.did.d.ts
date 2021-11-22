import type { Principal } from '@dfinity/principal';
export type Data = string;
export interface DataList {
  'insert' : (arg_0: Name, arg_1: Data) => Promise<undefined>,
  'lookup' : (arg_0: Name) => Promise<[] | [Data]>,
  'whoami' : () => Promise<string>,
}
export type Name = string;
export interface _SERVICE extends DataList {}
