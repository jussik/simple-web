import * as React from "react";
import {connect} from "react-redux";

import {IStoreState} from "../store";
import {addProduct} from "../store/products";
import {IProduct, Products} from "../api/products";

import Product from "./product";

type ActionProp<T> = (payload: T) => ReduxActions.Action<T>;

interface IProductListProps {
    products: IProduct[];
}
interface IProductListDispatchProps {
    onAdd: ActionProp<IProduct>;
}
class ProductList extends React.Component<IProductListProps & IProductListDispatchProps, {}> {
    createProduct() {
        const name = (this.refs["productName"] as HTMLInputElement).value;
        this.props.onAdd({ name });
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

const mapState = (state: IStoreState) => ({
    products: state.products
});
const mapDispatch = (dispatch: Redux.Dispatch<any>) => ({
    onAdd: (p: IProduct): any => Products.add(p).then(p => dispatch(addProduct(p)))
});
export default connect<IProductListProps, IProductListDispatchProps, {}>(mapState, mapDispatch)(ProductList);
