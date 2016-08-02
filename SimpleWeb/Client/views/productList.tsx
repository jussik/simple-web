import * as React from "react";
import {connect} from "react-redux";

import {IStoreState} from "../store";
import {addProduct} from "../store/products";
import {IProduct, Products} from "../api/products";

import Product from "./product";
import {ActionProp} from "./common";

interface IStateProps {
    products: IProduct[];
}
interface IDispatchProps {
    onAdd: ActionProp<IProduct>;
}
class ProductList extends React.Component<IStateProps & IDispatchProps, {}> {
    createProduct() {
        const input = this.refs["productName"] as HTMLInputElement;
        const name = input.value;
        input.value = "";
        if(name !== "") {
            this.props.onAdd({ name });
        }
    }
    render() {
        const createProduct = this.createProduct.bind(this);
        return <div>
            <h3>Products!</h3>
            {this.props.products.map(p => (
                <Product product={p} key={p.id}/>
            ))}
            <br/>
            <input type="text" ref="productName"/>
            <button onClick={createProduct}>Create</button>
        </div>;
    }
}

const stateMap = (state: IStoreState) => ({
    products: state.products
});
const dispatchMap = (dispatch: Redux.Dispatch<any>) => ({
    onAdd: (p: IProduct): any => Products.add(p).then(p => dispatch(addProduct(p)))
});
export default connect<IStateProps, IDispatchProps, {}>(stateMap, dispatchMap)(ProductList);
